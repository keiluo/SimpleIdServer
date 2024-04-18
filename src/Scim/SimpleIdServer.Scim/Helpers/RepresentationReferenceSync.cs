// Copyright (c) SimpleIdServer. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using SimpleIdServer.Scim.Domains;
using SimpleIdServer.Scim.DTOs;
using SimpleIdServer.Scim.Exceptions;
using SimpleIdServer.Scim.Persistence;
using SimpleIdServer.Scim.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleIdServer.Scim.Helpers
{
    public class RepresentationReferenceSync : IRepresentationReferenceSync
	{
		private readonly ISCIMAttributeMappingQueryRepository _scimAttributeMappingQueryRepository;
		private readonly ISCIMRepresentationCommandRepository _scimRepresentationCommandRepository;
		private readonly ISCIMSchemaCommandRepository _scimSchemaCommandRepository;

		public RepresentationReferenceSync(
			ISCIMAttributeMappingQueryRepository scimAttributeMappingQueryRepository,
			ISCIMRepresentationCommandRepository scimRepresentationCommandRepository,
			ISCIMSchemaCommandRepository scimSchemaCommandRepository)
        {
			_scimAttributeMappingQueryRepository = scimAttributeMappingQueryRepository;
			_scimRepresentationCommandRepository = scimRepresentationCommandRepository;
			_scimSchemaCommandRepository = scimSchemaCommandRepository;
		}

        public async Task<List<RepresentationSyncResult>> Sync(string resourceType, SCIMRepresentation newSourceScimRepresentation, ICollection<SCIMPatchResult> patchOperations, string location, SCIMSchema schema, bool updateAllReference = false)
        {
            var result = new RepresentationSyncResult();
            var attributeMappingLst = await _scimAttributeMappingQueryRepository.GetBySourceResourceType(resourceType);
            var sync = new List<RepresentationSyncResult>();
            if (!attributeMappingLst.Any()) sync.Add(result);

            var mappedSchemas = (await _scimSchemaCommandRepository.FindSCIMSchemaByResourceTypes(attributeMappingLst.Select(a => a.TargetResourceType).Union(attributeMappingLst.Select(a => a.SourceResourceType)).Distinct())).ToList();
            var selfReferenceAttribute = attributeMappingLst.FirstOrDefault(a => a.IsSelf);
            var propagatedAttribute = attributeMappingLst.FirstOrDefault(a => a.Mode == Mode.PROPAGATE_INHERITANCE);
            var mode = propagatedAttribute == null ? Mode.STANDARD : Mode.PROPAGATE_INHERITANCE;
            var allAdded = new List<RepresentationModified>();
            var allRemoved = new List<RepresentationModified>();

            // Update 'direct' references : GROUP => USER
            foreach (var kvp in attributeMappingLst.GroupBy(m => m.SourceAttributeId))
            {
                var sourceSchema = mappedSchemas.First(s => s.ResourceType == kvp.First().SourceResourceType && s.Attributes.Any(a => a.Id == kvp.Key)); 
                var allCurrentIds = patchOperations.Where(o => o.Attr.SchemaAttributeId == kvp.Key).SelectMany(pa => patchOperations.Where(p => p.Attr.ParentAttributeId == pa.Attr.Id && p.Attr.SchemaAttribute.Name == "value").Select(p => p.Attr.ValueString));
                var newIds = patchOperations
                    .Where(p => p.Operation == SCIMPatchOperations.ADD && p.Attr.SchemaAttributeId == kvp.Key)
                    .SelectMany(p => patchOperations.Where(po => po.Attr.ParentAttributeId == p.Attr.Id && po.Attr.SchemaAttribute.Name == SCIMConstants.StandardSCIMReferenceProperties.Value).Select(po => po.Attr.ValueString)).ToList();
                var idsToBeRemoved = patchOperations
                    .Where(p => p.Operation == SCIMPatchOperations.REMOVE && p.Attr.SchemaAttributeId == kvp.Key)
                    .SelectMany(p => patchOperations.Where(po => po.Attr.ParentAttributeId == p.Attr.Id && po.Attr.SchemaAttribute.Name == SCIMConstants.StandardSCIMReferenceProperties.Value).Select(po => po.Attr.ValueString)).ToList();
                var duplicateIds = allCurrentIds.GroupBy(i => i).Where(i => i.Count() > 1).ToList();
                if (duplicateIds.Any()) throw new SCIMUniquenessAttributeException(string.Format(Global.DuplicateReference, string.Join(",", duplicateIds.Select(_ => _.Key).Distinct())));
                foreach (var attributeMapping in kvp.Where(a => !a.IsSelf)) 
                {
                    var sourceAttribute = sourceSchema.GetAttributeById(attributeMapping.SourceAttributeId);
                    var targetSchema = mappedSchemas.First(s => s.ResourceType == attributeMapping.TargetResourceType && s.Attributes.Any(a => a.Id == attributeMapping.TargetAttributeId));
                    var targetAttribute = targetSchema.GetAttributeById(attributeMapping.TargetAttributeId);
                    var targetAttributeValue = targetSchema.GetChildren(targetAttribute).Single(a => a.Name == SCIMConstants.StandardSCIMReferenceProperties.Value);
                    var targetAttributeType = targetSchema.GetChildren(targetAttribute).SingleOrDefault(a => a.Name == SCIMConstants.StandardSCIMReferenceProperties.Type);
                    if (targetAttributeValue != null)
                    {
                        result = new RepresentationSyncResult();
                        var paginatedResult = await _scimRepresentationCommandRepository.FindGraphAttributes(idsToBeRemoved, newSourceScimRepresentation.Id, targetAttributeValue.Id);
                        result.RemoveReferenceAttributes(paginatedResult);

                        var referencedAttrLst = SCIMRepresentation.BuildHierarchicalAttributes(await _scimRepresentationCommandRepository.FindGraphAttributes(newIds, newSourceScimRepresentation.Id, targetAttributeValue.Id));
                        // Change indirect to direct.
                        foreach (var referencedAttr in referencedAttrLst)
                        {
                            var flatAttributes = referencedAttr.ToFlat();
                            var typeAttr = flatAttributes.Single(a => a.SchemaAttributeId == targetAttributeType.Id);
                            var valueAttr = flatAttributes.Single(a => a.SchemaAttributeId == targetAttributeValue.Id);
                            if(typeAttr.ValueString != "direct")
                            {
                                typeAttr.ValueString = "direct";
                                result.UpdateReferenceAttributes(new List<SCIMRepresentationAttribute> { typeAttr }, sync);
                            }
                        }

                        // Add reference attributes.
                        var missingReferenceAttributes = newIds.Where(i => !referencedAttrLst.Any(r => r.RepresentationId == i)).ToList();
                        var referencedRepresentations = (await _scimRepresentationCommandRepository.FindRepresentations(missingReferenceAttributes)).Where(r => r.ResourceType == attributeMapping.TargetResourceType);
                        foreach(var missingReferenceAttribute in missingReferenceAttributes)
                        {
                            var referencedRepresentation = referencedRepresentations.SingleOrDefault(r => r.Id == missingReferenceAttribute);
                            if (referencedRepresentation == null) continue;
                            var attr = BuildScimRepresentationAttribute(missingReferenceAttribute, attributeMapping.TargetAttributeId, newSourceScimRepresentation, mode, newSourceScimRepresentation.ResourceType, targetSchema);
                            result.AddReferenceAttributes(attr);
                            UpdateScimRepresentation(result, patchOperations, referencedRepresentation, schema, sourceAttribute);
                        }

                        var removedIds = result.RemovedRepresentationAttributes.Where(a => a.SchemaAttributeId == targetAttributeValue.Id).Select(r => r.RepresentationId).ToList();
                        var addedIds = result.AddedRepresentationAttributes.Where(a => a.SchemaAttributeId == targetAttributeValue.Id).Select(r => r.RepresentationId).ToList();
                        var notRemovedIds = idsToBeRemoved.Except(removedIds);
                        var notAddedIds = newIds.Except(addedIds);
                        allRemoved.AddRange(notRemovedIds.Select(id => new RepresentationModified(id, false)));
                        allRemoved.AddRange(removedIds.Select(id => new RepresentationModified(id, true)));
                        allAdded.AddRange(notAddedIds.Select(id => new RepresentationModified(id, false)));
                        allAdded.AddRange(addedIds.Select(id => new RepresentationModified(id, true)));
                        sync.Add(result);
                    }
                }

                // Update indirect references : GROUP => GROUP
                /*
                if(newIds.Any())
                    foreach(var attributeMapping in kvp.Where(a => a.IsSelf))
                    {
                        var sourceAttribute = schema.GetAttributeById(attributeMapping.SourceAttributeId);
                        var referencedRepresentations = await _scimRepresentationCommandRepository.FindRepresentations(newIds);
                        foreach (var newId in newIds)
                        {
                            var referencedRepresentation = referencedRepresentations.Single(r => r.Id == newId);
                            var attr = BuildScimRepresentationAttribute(newId, attributeMapping.TargetAttributeId, newSourceScimRepresentation, mode, newSourceScimRepresentation.ResourceType, schema);
                            result.AddReferenceAttributes(attr);
                            UpdateScimRepresentation(result, patchOperations, referencedRepresentation, schema, sourceAttribute);
                        }
                    }*/
            }

            var syncIndirectReferences = await SyncIndirectReferences(newSourceScimRepresentation, allAdded, allRemoved, propagatedAttribute, selfReferenceAttribute, mappedSchemas, sync, updateAllReference);
            sync.AddRange(syncIndirectReferences);
            return sync;
        }

        public virtual async Task<bool> IsReferenceProperty(ICollection<string> attributes)
        {
            var attrs = await _scimAttributeMappingQueryRepository.GetBySourceAttributes(attributes);
            return attributes.All(a => attrs.Any(at => at.SourceAttributeSelector == a));
        }

        protected async Task<List<RepresentationSyncResult>> SyncIndirectReferences(SCIMRepresentation newSourceScimRepresentation, List<RepresentationModified> allAdded, List<RepresentationModified> allRemoved, SCIMAttributeMapping propagatedAttribute, SCIMAttributeMapping selfReferenceAttribute, List<SCIMSchema> mappedSchemas, List<RepresentationSyncResult> sync, bool updateAllReference)
        {
            // Update 'indirect' references.
            var references = new List<RepresentationSyncResult>();
            if (propagatedAttribute != null && selfReferenceAttribute != null) 
            {
                var result = new RepresentationSyncResult();
                var sourceSchema = mappedSchemas.First(s => s.ResourceType == propagatedAttribute.SourceResourceType && s.Attributes.Any(a => a.Id == propagatedAttribute.SourceAttributeId));
                var targetSchema = mappedSchemas.First(s => s.ResourceType == propagatedAttribute.TargetResourceType && s.Attributes.Any(a => a.Id == propagatedAttribute.TargetAttributeId));
                var parentAttr = targetSchema.GetAttributeById(propagatedAttribute.TargetAttributeId);
                var selfParentAttr = sourceSchema.GetAttributeById(selfReferenceAttribute.SourceAttributeId);
                var valueAttr = targetSchema.GetChildren(parentAttr).Single(a => a.Name == SCIMConstants.StandardSCIMReferenceProperties.Value);
                var displayAttr = targetSchema.GetChildren(parentAttr).Single(a => a.Name == SCIMConstants.StandardSCIMReferenceProperties.Display);
                var selfValueAttr = sourceSchema.GetChildren(selfParentAttr).Single(a => a.Name == SCIMConstants.StandardSCIMReferenceProperties.Value);
                var addedDirectChildrenIds = allAdded.Where(r => r.IsDirect).Select(r => r.Id).ToList();
                var removedDirectChildrenIds = allRemoved.Where(r => r.IsDirect).Select(r => r.Id).ToList();
                var addedIndirectChildrenIds = allAdded.Where(r => !r.IsDirect).Select(r => r.Id).ToList();
                var removedIndirectChildrenIds = allRemoved.Where(r => !r.IsDirect).Select(r => r.Id).ToList();

                List<SCIMRepresentation> allParents = null, allSelfChildren = null; ;
                if(updateAllReference)
                {
                    var propagatedChildren = await _scimRepresentationCommandRepository.FindGraphAttributes(newSourceScimRepresentation.Id, valueAttr.Id);
                    var typeAttrs = propagatedChildren.Where(c => c.SchemaAttributeId == displayAttr.Id && !addedDirectChildrenIds.Contains(c.RepresentationId)).ToList();
                    foreach (var ta in typeAttrs)
                        ta.ValueString = newSourceScimRepresentation.DisplayName;

                    result.UpdateReferenceAttributes(typeAttrs, sync);
                    references.Add(result);
                }

                // Add indirect reference to the parent.
                if (addedDirectChildrenIds.Any())
                {
                    result = new RepresentationSyncResult();
                    allParents = await GetParents(new List<SCIMRepresentation> { newSourceScimRepresentation }, selfReferenceAttribute);
                    var existingParentReferencedIds = (await _scimRepresentationCommandRepository.FindAttributesBySchemaAttributeAndValues(valueAttr.Id, allParents.Select(p => p.Id), CancellationToken.None)).Select(p => p.ValueString).ToList();
                    foreach(var parent in allParents.Where(p => !existingParentReferencedIds.Contains(p.Id)))
                        foreach(var addedId in addedDirectChildrenIds)
                            result.AddReferenceAttributes(BuildScimRepresentationAttribute(addedId, propagatedAttribute.TargetAttributeId, parent, Mode.PROPAGATE_INHERITANCE, parent.ResourceType, targetSchema, false));

                    references.Add(result);
                }

                // If at least one parent has a reference to the child then add indirect reference to the child.
                // Otherwise remove all the indirect references.
                if (removedDirectChildrenIds.Any())
                {
                    result = new RepresentationSyncResult();
                    allParents ??= await GetParents(new List<SCIMRepresentation> { newSourceScimRepresentation }, selfReferenceAttribute);
                    allSelfChildren = await GetChildren(new List<SCIMRepresentation> { newSourceScimRepresentation }, selfReferenceAttribute);
                    var allTargetIds = allSelfChildren.Where(r => r.ResourceType == targetSchema.ResourceType).Select(r => r.Id).ToList();
                    var allSelfIds = allSelfChildren.Where(r => r.ResourceType == sourceSchema.ResourceType).Select(r => r.Id).ToList();
                    var targets = await _scimRepresentationCommandRepository.FindGraphAttributesBySchemaAttributeId(allTargetIds, parentAttr.Id, CancellationToken.None);
                    foreach (var removedDirectChild in removedDirectChildrenIds)
                    {
                        var referencedParentIds = targets.Where(t => t.RepresentationId == removedDirectChild && t.SchemaAttributeId == valueAttr.Id).Select(t => t.ValueString);
                        if(referencedParentIds.Any(i => allSelfIds.Contains(i)))
                            result.AddReferenceAttributes(BuildScimRepresentationAttribute(removedDirectChild, propagatedAttribute.TargetAttributeId, newSourceScimRepresentation, Mode.PROPAGATE_INHERITANCE, newSourceScimRepresentation.ResourceType, targetSchema, false));
                        else
                        {
                            var hierarchicalTargetAttrs = SCIMRepresentation.BuildHierarchicalAttributes(targets.Where(t => t.RepresentationId == removedDirectChild));
                            foreach (var parent in allParents)
                            {
                                var attrToBeRemove = hierarchicalTargetAttrs.FirstOrDefault(t => t.CachedChildren.Any(c => c.ValueString == parent.Id && c.SchemaAttributeId == valueAttr.Id));
                                if (attrToBeRemove != null)
                                    result.RemoveReferenceAttributes(attrToBeRemove.ToFlat());
                            }
                        }
                    }

                    references.Add(result);
                }

                // Populate the children.
                if (removedIndirectChildrenIds.Any() || addedIndirectChildrenIds.Any())
                {
                    // Refactor this part in order to improve the performance.
                    result = new RepresentationSyncResult();
                    var fullPath = $"{selfReferenceAttribute.SourceAttributeSelector}.{SCIMConstants.StandardSCIMReferenceProperties.Value}";
                    var existingChildrenIds = (await _scimRepresentationCommandRepository.FindAttributesByAproximativeFullPath(newSourceScimRepresentation.Id, fullPath, CancellationToken.None)).Select(f => f.ValueString);
                    var allIds = new List<string>();
                    allIds.AddRange(addedIndirectChildrenIds);
                    allIds.AddRange(removedIndirectChildrenIds);
                    var notRemovedChildrenIds = existingChildrenIds.Except(removedIndirectChildrenIds).ToList();
                    var indirectChildren = await _scimRepresentationCommandRepository.FindRepresentations(allIds);
                    var notRemoved = await _scimRepresentationCommandRepository.FindRepresentations(notRemovedChildrenIds);

                    var notRemovedChildren = new List<SCIMRepresentation>();
                    notRemovedChildren.AddRange(await GetChildren(notRemoved, selfReferenceAttribute));

                    var notRemovedChldIds = notRemovedChildren.Select(r => r.Id);

                    foreach (var indirectChild in indirectChildren)
                    {
                        var allChld = await GetChildren(new List<SCIMRepresentation> { indirectChild }, selfReferenceAttribute);
                        foreach (var children in await ResolvePropagatedChildren(newSourceScimRepresentation.Id, indirectChild, selfReferenceAttribute, valueAttr, allChld))
                        {
                            if (addedIndirectChildrenIds.Contains(indirectChild.Id))
                            {
                                var parentAttrs = children.Where(c => c.SchemaAttributeId == propagatedAttribute.TargetAttributeId);
                                foreach (var pa in parentAttrs)
                                {
                                    if (!children.Any(r => r.ParentAttributeId == pa.Id && r.SchemaAttributeId == valueAttr.Id && r.ValueString == newSourceScimRepresentation.Id))
                                        result.AddReferenceAttributes(BuildScimRepresentationAttribute(pa.RepresentationId, propagatedAttribute.TargetAttributeId, newSourceScimRepresentation, Mode.PROPAGATE_INHERITANCE, newSourceScimRepresentation.ResourceType, targetSchema, false));
                                }
                            }
                            else
                            {
                                var representationIds = children.Select(c => c.RepresentationId).Distinct();
                                foreach (var representationId in representationIds)
                                {
                                    bool atLeastOneParent = false;
                                    SCIMRepresentationAttribute attrToRemove = null;
                                    var parentAttrs = children.Where(c => c.SchemaAttributeId == propagatedAttribute.TargetAttributeId && c.RepresentationId == representationId);
                                    foreach (var pa in parentAttrs)
                                    {
                                        var subAttrs = children.Where(c => c.ParentAttributeId == pa.Id).ToList();
                                        if (subAttrs.Any(a => a.SchemaAttributeId == valueAttr.Id && notRemovedChldIds.Contains(a.ValueString)))
                                        {
                                            atLeastOneParent = true;
                                            break;
                                        }

                                        if (subAttrs.Any(a => a.SchemaAttributeId == valueAttr.Id && a.ValueString == newSourceScimRepresentation.Id))
                                            attrToRemove = pa;
                                    }

                                    if (!atLeastOneParent && attrToRemove != null)
                                    {
                                        result.RemoveReferenceAttributes(children.Where(a => a.ParentAttributeId == attrToRemove.Id).ToList());
                                        result.RemoveReferenceAttributes(new List<SCIMRepresentationAttribute> { attrToRemove });
                                    }
                                }
                            }
                        }
                    }

                    references.Add(result);
                }
            }

            return references;
        }

        protected virtual async Task<List<SCIMRepresentation>> GetChildren(List<SCIMRepresentation> scimRepresentations, SCIMAttributeMapping selfReferenceAttribute) 
        {
            var childrenIds = new List<string>();
            await ResolveChildrenIds(scimRepresentations.Select(p => p.Id).ToList(), selfReferenceAttribute, childrenIds);
            return await _scimRepresentationCommandRepository.FindRepresentations(childrenIds);
        }

        protected virtual async Task<List<SCIMRepresentation>> GetParents(List<SCIMRepresentation> scimRepresentations, SCIMAttributeMapping selfReferenceAttribute) 
        {
            var parentIds = new List<string>();
            await ResolveParentIds(scimRepresentations.Select(p => p.Id).ToList(), selfReferenceAttribute, parentIds);
            return await _scimRepresentationCommandRepository.FindRepresentations(parentIds);
        }

        private async Task ResolveParentIds(List<string> ids,SCIMAttributeMapping selfReferenceAttribute, List<string> representationIds)
        {
            var fullPath = $"{selfReferenceAttribute.SourceAttributeSelector}.{SCIMConstants.StandardSCIMReferenceProperties.Value}";
            var parents = await _scimRepresentationCommandRepository.FindAttributesByExactFullPathAndValues(fullPath, ids, CancellationToken.None);
            var newParents = parents.Where(p => !representationIds.Contains(p.RepresentationId));
            if (!newParents.Any()) return;
            foreach (var parent in newParents)
                representationIds.Add(parent.RepresentationId);

            await ResolveParentIds(newParents.Select(p => p.RepresentationId).ToList(), selfReferenceAttribute, representationIds);
        }

        private async Task ResolveChildrenIds(List<string> ids, SCIMAttributeMapping selfReferenceAttribute, List<string> representationIds) 
        {
            var fullPath = $"{selfReferenceAttribute.SourceAttributeSelector}.{SCIMConstants.StandardSCIMReferenceProperties.Value}";
            // members.value => for all the representationIds
            var children = await _scimRepresentationCommandRepository.FindAttributesByExactFullPathAndRepresentationIds(fullPath, ids, CancellationToken.None);
            var newChildren = children.Where(p => !representationIds.Contains(p.ValueString));
            if(!newChildren.Any()) return;
            foreach (var child in newChildren)
                representationIds.Add(child.ValueString);
            await ResolveChildrenIds(newChildren.Select(p => p.ValueString).ToList(), selfReferenceAttribute, representationIds);
        }

        protected virtual async Task<List<List<SCIMRepresentationAttribute>>> ResolvePropagatedChildren(string sourceRepresentationId, SCIMRepresentation scimRepresentation, SCIMAttributeMapping selfReferenceAttribute, SCIMSchemaAttribute targetAttributeId, List<SCIMRepresentation> allChildren)
        {
            var fullPath = $"{selfReferenceAttribute.SourceAttributeSelector}.{SCIMConstants.StandardSCIMReferenceProperties.Value}";
            var attrs = await _scimRepresentationCommandRepository.FindAttributesByExactFullPathAndRepresentationIds(fullPath, new List<string> { scimRepresentation.Id }, CancellationToken.None);
            var childrenIds = attrs.Select(f => f.ValueString).ToList();
            var children = new List<List<SCIMRepresentationAttribute>>
            {
                await _scimRepresentationCommandRepository.FindGraphAttributes(childrenIds, scimRepresentation.Id, targetAttributeId.Id, sourceRepresentationId: sourceRepresentationId)
            };

            foreach (var child in allChildren.Where(c => childrenIds.Contains(c.Id) && c.ResourceType == selfReferenceAttribute.SourceResourceType)) 
            {
                children.AddRange(await ResolvePropagatedChildren(sourceRepresentationId, child, selfReferenceAttribute, targetAttributeId, allChildren));
            }

            return children;
        }

        protected class RepresentationModified
        {
            public RepresentationModified(string id, bool isDirect)
            {
                Id = id;
                IsDirect = isDirect;
            }

            public string Id { get; private set; }
            public bool IsDirect { get; private set; }
        }

        protected virtual void UpdateScimRepresentation(RepresentationSyncResult result, ICollection<SCIMPatchResult> patches, SCIMRepresentation targetRepresentation, SCIMSchema sourceSchema, SCIMSchemaAttribute sourceAttribute)
        {
            var sourceAttributeValue = sourceSchema.GetChildren(sourceAttribute).Single(a => a.Name == SCIMConstants.StandardSCIMReferenceProperties.Value);
            var sourceAttributeType = sourceSchema.GetChildren(sourceAttribute).SingleOrDefault(a => a.Name == SCIMConstants.StandardSCIMReferenceProperties.Type);
            var sourceAttributeDisplay = sourceSchema.GetChildren(sourceAttribute).SingleOrDefault(a => a.Name == SCIMConstants.StandardSCIMReferenceProperties.Display);
            var flatAttrs = patches.Select(p => p.Attr);
            var representationId = flatAttrs.First().RepresentationId;
            var valAttr = flatAttrs.Single(a => a.SchemaAttributeId == sourceAttributeValue.Id && a.ValueString == targetRepresentation.Id);
            if (sourceAttributeType != null)
            {
                result.AddReferenceAttributes(new List<SCIMRepresentationAttribute>
                {
                    new SCIMRepresentationAttribute(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), sourceAttributeType, sourceAttributeType.SchemaId)
                    {
                        ValueString = targetRepresentation.ResourceType,
                        ParentAttributeId = valAttr.ParentAttributeId,
                        RepresentationId = representationId,
                        IsComputed = true
                    }
                });
            }

            if (sourceAttributeDisplay != null)
            {
                result.AddReferenceAttributes(new List<SCIMRepresentationAttribute>
                {
                    new SCIMRepresentationAttribute(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), sourceAttributeDisplay, sourceAttributeDisplay.SchemaId)
                    {
                        ValueString = targetRepresentation.DisplayName,
                        ParentAttributeId = valAttr.ParentAttributeId,
                        RepresentationId = representationId,
                        IsComputed = true
                    }
                });
            }
        }

        public List<SCIMRepresentationAttribute> BuildScimRepresentationAttribute(string representationId, string attributeId, SCIMRepresentation sourceRepresentation, Mode mode, string sourceResourceType, SCIMSchema targetSchema, bool isDirect = true)
        {
            var attributes = new List<SCIMRepresentationAttribute>();
            var targetSchemaAttribute = targetSchema.GetAttributeById(attributeId);
            var values = targetSchema.GetChildren(targetSchemaAttribute);
            var value = values.FirstOrDefault(s => s.Name == SCIMConstants.StandardSCIMReferenceProperties.Value);
            var display = values.FirstOrDefault(s => IsDisplayName(s.Name));
            var type = values.FirstOrDefault(s => s.Name == SCIMConstants.StandardSCIMReferenceProperties.Type);

            var parentAttr = new SCIMRepresentationAttribute(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), targetSchemaAttribute, targetSchemaAttribute.SchemaId)
            {
                SchemaAttribute = targetSchemaAttribute,
                RepresentationId = representationId
            };
            attributes.Add(parentAttr);

            if (value != null)
            {
                attributes.Add(new SCIMRepresentationAttribute(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), value, value.SchemaId)
                {
                    ValueString = sourceRepresentation.Id,
                    RepresentationId = representationId,
                    ParentAttributeId = parentAttr.Id
                });
            }

            if (display != null)
            {
                attributes.Add(new SCIMRepresentationAttribute(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), display, display.SchemaId)
                {
                    ValueString = sourceRepresentation.DisplayName,
                    RepresentationId = representationId,
                    IsComputed = true,
                    ParentAttributeId = parentAttr.Id
                });
            }

            if (type != null)
            {
                switch (mode)
                {
                    case Mode.STANDARD:
                        attributes.Add(new SCIMRepresentationAttribute(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), type, type.SchemaId)
                        {
                            ValueString = sourceResourceType,
                            RepresentationId = representationId,
                            IsComputed = true,
                            ParentAttributeId = parentAttr.Id
                        });
                        break;
                    case Mode.PROPAGATE_INHERITANCE:
                        attributes.Add(new SCIMRepresentationAttribute(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), type, type.SchemaId)
                        {
                            ValueString = isDirect ? "direct" : "indirect",
                            RepresentationId = representationId,
                            IsComputed = true,
                            ParentAttributeId = parentAttr.Id
                        });
                        break;
                }
            }

            return attributes;
        }

        private static bool IsDisplayName(string name)
        {
			return name == SCIMConstants.StandardSCIMReferenceProperties.Display || name == SCIMConstants.StandardSCIMReferenceProperties.DisplayName;
		}

        private class RepresentationTreeNode
        {
            public string RepresentationId { get; set; }
            public ICollection<RepresentationTreeNode> Nodes { get; set; }
        }
    }
}
