﻿// Copyright (c) SimpleIdServer. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SimpleIdServer.Did;
using SimpleIdServer.IdServer.Domains;
using SimpleIdServer.IdServer.Domains.DTOs;
using SimpleIdServer.IdServer.DTOs;
using SimpleIdServer.IdServer.Exceptions;
using SimpleIdServer.IdServer.ExternalEvents;
using SimpleIdServer.IdServer.Jwt;
using SimpleIdServer.IdServer.Store;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleIdServer.IdServer.Api.Users
{
    public class UsersController : BaseController
    {
        private readonly IUserRepository _userRepository;
        private readonly IRealmRepository _realmRepository;
        private readonly IGroupRepository _groupRepository;
        private readonly IBusControl _busControl;
        private readonly IJwtBuilder _jwtBuilder;
        private readonly ILogger<UsersController> _logger;
        private readonly IEnumerable<IDIDGenerator> _generators;

        public UsersController(IUserRepository userRepository, IRealmRepository realmRepository, IGroupRepository groupRepository, IBusControl busControl, IJwtBuilder jwtBuilder, ILogger<UsersController> logger, IEnumerable<IDIDGenerator> generators)
        {
            _userRepository = userRepository;
            _realmRepository = realmRepository;
            _groupRepository = groupRepository;
            _busControl = busControl;
            _jwtBuilder = jwtBuilder;
            _logger = logger;
            _generators = generators;
        }

        #region Querying

        [HttpPost]
        public async Task<IActionResult> Search([FromRoute] string prefix, [FromBody] SearchRequest request)
        {
            prefix = prefix ?? Constants.DefaultRealm;
            try
            {
                CheckAccessToken(prefix, Constants.StandardScopes.Users.Name, _jwtBuilder);
                var query = _userRepository.Query()
                    .Include(u => u.Realms)
                    .Include(u => u.OAuthUserClaims)
                    .Where(u => u.Realms.Any(r => r.RealmsName == prefix)).AsNoTracking();
                if (!string.IsNullOrWhiteSpace(request.Filter))
                    query = query.Where(request.Filter);

                if (!string.IsNullOrWhiteSpace(request.OrderBy))
                    query = query.OrderBy(request.OrderBy);
                else
                    query = query.OrderByDescending(u => u.UpdateDateTime);

                var count = query.Count();
                var users = await query.Skip(request.Skip.Value).Take(request.Take.Value).ToListAsync(CancellationToken.None);
                return new OkObjectResult(new SearchResult<User>
                {
                    Content = users,
                    Count = count
                });
            }
            catch (OAuthException ex)
            {
                return BuildError(ex);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromRoute] string prefix, string id, CancellationToken cancellationToken)
        {
            try
            {
                prefix = prefix ?? Constants.DefaultRealm;
                CheckAccessToken(prefix, Constants.StandardScopes.Users.Name, _jwtBuilder);
                var user = await _userRepository.Get(us => us
                    .Include(u => u.OAuthUserClaims)
                    .Include(u => u.Credentials)
                    .Include(u => u.CredentialOffers)
                    .Include(u => u.Groups)
                    .Include(u => u.Consents)
                    .Include(u => u.ExternalAuthProviders)
                    .Include(u => u.Realms)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == id && u.Realms.Any(r => r.RealmsName == prefix), cancellationToken));
                if (user == null) return new NotFoundResult();
                return new OkObjectResult(user);
            }
            catch (OAuthException ex)
            {
                _logger.LogError(ex.ToString());
                return BuildError(ex);
            }
        }

        [HttpGet]
        public async Task<IActionResult> ResolveRoles([FromRoute] string prefix, string id, CancellationToken cancellationToken)
        {
            try
            {
                prefix = prefix ?? Constants.DefaultRealm;
                CheckAccessToken(prefix, Constants.StandardScopes.Users.Name, _jwtBuilder);
                var user = await _userRepository.Get(us => us
                    .Include(u => u.Groups)
                    .Include(u => u.Realms)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == id && u.Realms.Any(r => r.RealmsName == prefix), cancellationToken));
                if (user == null) return new NotFoundResult();
                var grpPathLst = user.Groups.SelectMany(g => g.ResolveAllPath()).Distinct();
                var allGroups = await _groupRepository.Query()
                    .Include(g => g.Roles)
                    .AsNoTracking()
                    .Where(g => grpPathLst.Contains(g.FullPath))
                    .ToListAsync();
                var roles = allGroups.SelectMany(g => g.Roles).Select(r => r.Name).Distinct();
                return new OkObjectResult(roles);
            }
            catch (OAuthException ex)
            {
                _logger.LogError(ex.ToString());
                return BuildError(ex);
            }
        }

        #endregion

        #region CRUD

        [HttpPost]
        public async Task<IActionResult> Add([FromRoute] string prefix, [FromBody] RegisterUserRequest request, CancellationToken cancellationToken)
        {
            using (var activity = Tracing.IdServerActivitySource.StartActivity("Add user"))
            {
                try
                {
                    prefix = prefix ?? Constants.DefaultRealm;
                    CheckAccessToken(prefix, Constants.StandardScopes.Users.Name, _jwtBuilder);
                    var realm = await _realmRepository.Query().FirstAsync(r => r.Name == prefix, cancellationToken);
                    await Validate();
                    var newUser = new User
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = request.Name,
                        Firstname = request.Firstname,
                        Lastname = request.Lastname,
                        Email = request.Email,
                        OAuthUserClaims = request.Claims?.Select(c => new UserClaim
                        {
                            Id = Guid.NewGuid().ToString(),
                            Name = c.Key,
                            Value = c.Value
                        }).ToList(),
                        UpdateDateTime = DateTime.UtcNow,
                        CreateDateTime = DateTime.UtcNow
                    };
                    newUser.Realms.Add(new RealmUser
                    {
                        Realm = realm
                    });
                    _userRepository.Add(newUser);
                    await _userRepository.SaveChanges(cancellationToken);
                    activity?.SetStatus(ActivityStatusCode.Ok, "Add user success");
                    await _busControl.Publish(new AddUserSuccessEvent
                    {
                        Realm = prefix,
                        Id = newUser.Id,
                        Name = newUser.Name,
                        Email = newUser.Email,
                        Firstname = newUser.Firstname,
                        Lastname = newUser.Lastname,
                        Claims = request.Claims
                    });
                    return new ContentResult
                    {
                        Content = JsonSerializer.Serialize(newUser),
                        ContentType = "application/json",
                        StatusCode = (int)HttpStatusCode.Created
                    };
                }
                catch (OAuthException ex)
                {
                    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                    return BuildError(ex);
                }
            }

            async Task Validate()
            {
                if (string.IsNullOrWhiteSpace(request.Name)) throw new OAuthException(ErrorCodes.INVALID_REQUEST, string.Format(ErrorMessages.MISSING_PARAMETER, UserNames.Name));
                if ((await _userRepository.GetAll(us => us.AsNoTracking().Where(u => u.Name == request.Name).ToListAsync())).Any()) throw new OAuthException(ErrorCodes.INVALID_REQUEST, string.Format(ErrorMessages.USER_EXISTS, request.Name));
            }
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromRoute] string prefix, string id, [FromBody] UpdateUserRequest request)
        {
            prefix = prefix ?? Constants.DefaultRealm;
            using (var activity = Tracing.IdServerActivitySource.StartActivity("Update user"))
            {
                try
                {
                    CheckAccessToken(prefix, Constants.StandardScopes.Users.Name, _jwtBuilder);
                    if (request == null) throw new OAuthException(HttpStatusCode.BadRequest, ErrorCodes.INVALID_REQUEST, ErrorMessages.INVALID_INCOMING_REQUEST);
                    var user = await _userRepository
                        .Query()
                        .Include(u => u.OAuthUserClaims)
                        .Include(u => u.Realms)
                        .SingleOrDefaultAsync(a => a.Id == id && a.Realms.Any(r => r.RealmsName == prefix));
                    if (user == null) throw new OAuthException(HttpStatusCode.NotFound, ErrorCodes.NOT_FOUND, string.Format(ErrorMessages.UNKNOWN_USER, id));
                    user.UpdateEmail(request.Email);
                    user.UpdateName(request.Name);
                    user.UpdateLastname(request.Lastname);
                    user.NotificationMode = request.NotificationMode;
                    user.UpdateDateTime = DateTime.UtcNow;
                    await _userRepository.SaveChanges(CancellationToken.None);
                    activity?.SetStatus(ActivityStatusCode.Ok, "User is updated");
                    await _busControl.Publish(new UpdateUserSuccessEvent
                    {
                        Realm = prefix,
                        Id = id
                    });
                    return new NoContentResult();
                }
                catch (OAuthException ex)
                {
                    _logger.LogError(ex.ToString());
                    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                    await _busControl.Publish(new UpdateUserFailureEvent
                    {
                        Realm = prefix,
                        Id = id
                    });
                    return BuildError(ex);
                }
            }
        }

        [HttpDelete]
        public async Task<IActionResult> Delete([FromRoute] string prefix, string id, CancellationToken cancellationToken)
        {
            using (var activity = Tracing.IdServerActivitySource.StartActivity("Remove user"))
            {
                try
                {
                    prefix = prefix ?? Constants.DefaultRealm;
                    CheckAccessToken(prefix, Constants.StandardScopes.Users.Name, _jwtBuilder);
                    var user = await _userRepository.Get(us => us.Include(u => u.Realms).FirstOrDefaultAsync(u => u.Id == id && u.Realms.Any(r => r.RealmsName == prefix), cancellationToken));
                    if (user == null) return new NotFoundResult();
                    _userRepository.Remove(new List<User> { user });
                    await _userRepository.SaveChanges(cancellationToken);
                    activity?.SetStatus(ActivityStatusCode.Ok, "User is removed");
                    await _busControl.Publish(new RemoveUserSuccessEvent
                    {
                        Realm = prefix,
                        Id = id,
                        Name = user.Name
                    });
                    return new NoContentResult();
                }
                catch (OAuthException ex)
                {
                    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                    return BuildError(ex);
                }
            }
        }

        #endregion

        #region Credentials

        [HttpPost]
        public async Task<IActionResult> AddCredential([FromBody] string prefix, string id, [FromBody] AddUserCredentialRequest request)
        {
            prefix = prefix ?? Constants.DefaultRealm;
            using (var activity = Tracing.IdServerActivitySource.StartActivity("Add user's credential"))
            {
                try
                {
                    CheckAccessToken(prefix, Constants.StandardScopes.Users.Name, _jwtBuilder);
                    if (request == null) throw new OAuthException(HttpStatusCode.BadRequest, ErrorCodes.INVALID_REQUEST, ErrorMessages.INVALID_INCOMING_REQUEST);
                    var user = await _userRepository
                        .Query()
                        .Include(u => u.Credentials)
                        .Include(u => u.Realms)
                        .SingleOrDefaultAsync(a => a.Id == id && a.Realms.Any(r => r.RealmsName == prefix));
                    if (user == null) throw new OAuthException(HttpStatusCode.NotFound, ErrorCodes.NOT_FOUND, string.Format(ErrorMessages.UNKNOWN_USER, id));
                    if (request.Active)
                    {
                        foreach (var act in user.Credentials.Where(c => c.CredentialType == request.Credential.CredentialType))
                            act.IsActive = false;
                        request.Credential.IsActive = true;
                    }

                    request.Credential.Id = Guid.NewGuid().ToString();
                    user.Credentials.Add(request.Credential);
                    user.UpdateDateTime = DateTime.UtcNow;
                    await _userRepository.SaveChanges(CancellationToken.None);
                    activity?.SetStatus(ActivityStatusCode.Ok, "User's credential is added");
                    return new ContentResult
                    {
                        Content = JsonSerializer.Serialize(request.Credential),
                        ContentType = "application/json",
                        StatusCode = (int)HttpStatusCode.Created
                    };
                }
                catch (OAuthException ex)
                {
                    _logger.LogError(ex.ToString());
                    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                    return BuildError(ex);
                }
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateCredential([FromRoute] string prefix, string id, string credentialId, [FromBody] UpdateUserCredentialRequest request, CancellationToken cancellationToken)
        {
            using (var activity = Tracing.IdServerActivitySource.StartActivity("Update credential"))
            {
                try
                {
                    prefix = prefix ?? Constants.DefaultRealm;
                    CheckAccessToken(prefix, Constants.StandardScopes.Users.Name, _jwtBuilder);
                    if (string.IsNullOrWhiteSpace(request.Value)) throw new OAuthException(ErrorCodes.INVALID_REQUEST, string.Format(ErrorMessages.MISSING_PARAMETER, UserCredentialNames.Value));
                    var user = await _userRepository.Get(us => us
                        .Include(u => u.Realms)
                        .Include(u => u.Credentials)
                        .FirstOrDefaultAsync(u => u.Id == id && u.Realms.Any(r => r.RealmsName == prefix), cancellationToken));
                    if (user == null) return new NotFoundResult();
                    var existingCredential = user.Credentials.SingleOrDefault(c => c.Id == credentialId);
                    if (existingCredential == null) throw new OAuthException(ErrorCodes.INVALID_REQUEST, string.Format(ErrorMessages.UNKNOWN_USER_CREDENTIAL, credentialId));
                    existingCredential.Value = request.Value;
                    existingCredential.OTPAlg = request.OTPAlg;
                    user.UpdateDateTime = DateTime.UtcNow;
                    await _userRepository.SaveChanges(cancellationToken);
                    activity?.SetStatus(ActivityStatusCode.Ok, "Credential is replaced");
                    await _busControl.Publish(new UpdateUserCredentialSuccessEvent
                    {
                        Realm = prefix,
                        Name = user.Name
                    });
                    return new NoContentResult();
                }
                catch (OAuthException ex)
                {
                    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                    return BuildError(ex);
                }
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteCredential([FromRoute] string prefix, string id, string credentialId)
        {
            using (var activity = Tracing.IdServerActivitySource.StartActivity("Remove credential"))
            {
                try
                {
                    prefix = prefix ?? Constants.DefaultRealm;
                    CheckAccessToken(prefix, Constants.StandardScopes.Users.Name, _jwtBuilder);
                    var user = await _userRepository.Get(us => us
                        .Include(u => u.Realms)
                        .Include(u => u.Credentials)
                        .FirstOrDefaultAsync(u => u.Id == id && u.Realms.Any(r => r.RealmsName == prefix)));
                    if (user == null) return new NotFoundResult();
                    var existingCredential = user.Credentials.SingleOrDefault(c => c.Id == credentialId);
                    if (existingCredential == null) throw new OAuthException(ErrorCodes.INVALID_REQUEST, string.Format(ErrorMessages.UNKNOWN_USER_CREDENTIAL, credentialId));
                    user.Credentials.Remove(existingCredential);
                    user.UpdateDateTime = DateTime.UtcNow;
                    await _userRepository.SaveChanges(CancellationToken.None);
                    activity?.SetStatus(ActivityStatusCode.Ok, "Credential is removed");
                    return new NoContentResult();
                }
                catch (OAuthException ex)
                {
                    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                    return BuildError(ex);
                }
            }
        }

        [HttpGet]
        public async Task<IActionResult> DefaultCredential([FromRoute] string prefix, string id, string credentialId)
        {
            using (var activity = Tracing.IdServerActivitySource.StartActivity("Set default credential"))
            {
                try
                {
                    prefix = prefix ?? Constants.DefaultRealm;
                    CheckAccessToken(prefix, Constants.StandardScopes.Users.Name, _jwtBuilder);
                    var user = await _userRepository.Get(us => us
                        .Include(u => u.Realms)
                        .Include(u => u.Credentials)
                        .FirstOrDefaultAsync(u => u.Id == id && u.Realms.Any(r => r.RealmsName == prefix)));
                    if (user == null) return new NotFoundResult();
                    var existingCredential = user.Credentials.SingleOrDefault(c => c.Id == credentialId);
                    if (existingCredential == null) throw new OAuthException(ErrorCodes.INVALID_REQUEST, string.Format(ErrorMessages.UNKNOWN_USER_CREDENTIAL, credentialId));
                    foreach (var cred in user.Credentials.Where(c => c.CredentialType == existingCredential.CredentialType))
                        existingCredential.IsActive = false;
                    existingCredential.IsActive = true;
                    user.UpdateDateTime = DateTime.UtcNow;
                    await _userRepository.SaveChanges(CancellationToken.None);
                    activity?.SetStatus(ActivityStatusCode.Ok, "Default credential is set");
                    return new NoContentResult();
                }
                catch (OAuthException ex)
                {
                    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                    return BuildError(ex);
                }
            }
        }

        #endregion

        #region Claims

        [HttpPut]
        public async Task<IActionResult> UpdateClaims([FromRoute] string prefix, string id, [FromBody] UpdateUserClaimsRequest request, CancellationToken cancellationToken)
        {
            using (var activity = Tracing.IdServerActivitySource.StartActivity("Update claims"))
            {
                try
                {
                    prefix = prefix ?? Constants.DefaultRealm;
                    CheckAccessToken(prefix, Constants.StandardScopes.Users.Name, _jwtBuilder);
                    Validate();
                    var user = await _userRepository.Get(us => us
                        .Include(u => u.Realms)
                        .Include(u => u.OAuthUserClaims)
                        .FirstOrDefaultAsync(u => u.Id == id && u.Realms.Any(r => r.RealmsName == prefix), cancellationToken));
                    if (user == null) throw new OAuthException(HttpStatusCode.NotFound, ErrorCodes.NOT_FOUND, string.Format(ErrorMessages.UNKNOWN_USER, id));
                    Update(user, request);
                    await _userRepository.SaveChanges(cancellationToken);
                    activity?.SetStatus(ActivityStatusCode.Ok, "Claims are updated");
                    await _busControl.Publish(new UpdateUserClaimsSuccessEvent
                    {
                        Realm = prefix,
                        Id = id
                    });
                    return new NoContentResult();
                }
                catch (OAuthException ex)
                {
                    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                    await _busControl.Publish(new UpdateUserClaimsFailureEvent
                    {
                        Realm = prefix,
                        Id = id
                    });
                    return BuildError(ex);
                }

                void Validate()
                {
                    if (request == null) throw new OAuthException(ErrorCodes.INVALID_REQUEST, ErrorMessages.INVALID_INCOMING_REQUEST);
                }

                void Update(User user, UpdateUserClaimsRequest request)
                {
                    user.OAuthUserClaims.Clear();
                    foreach (var cl in request.Claims)
                        user.OAuthUserClaims.Add(new UserClaim { Id = Guid.NewGuid().ToString(), Name = cl.Name, Value = cl.Value });
                }
            }
        }

        #endregion

        #region Groups

        [HttpPost]
        public async Task<IActionResult> AddGroup([FromRoute] string prefix, string id, string groupId)
        {
            prefix = prefix ?? Constants.DefaultRealm;
            using (var activity = Tracing.IdServerActivitySource.StartActivity("Add user's group"))
            {
                try
                {
                    CheckAccessToken(prefix, Constants.StandardScopes.Users.Name, _jwtBuilder);
                    var user = await _userRepository
                        .Query()
                        .Include(u => u.Groups)
                        .Include(u => u.Realms)
                        .SingleOrDefaultAsync(a => a.Id == id && a.Realms.Any(r => r.RealmsName == prefix));
                    if (user == null) throw new OAuthException(HttpStatusCode.NotFound, ErrorCodes.NOT_FOUND, string.Format(ErrorMessages.UNKNOWN_USER, id));
                    var newGroup = await _groupRepository.Query()
                        .Include(g => g.Realms)
                        .SingleOrDefaultAsync(a => a.Id == groupId && a.Realms.Any(r => r.Name == prefix)); ;
                    if (newGroup == null) throw new OAuthException(HttpStatusCode.BadRequest, ErrorCodes.INVALID_REQUEST, string.Format(ErrorMessages.UNKNOWN_USER_GROUP, groupId));
                    user.Groups.Add(newGroup);
                    user.UpdateDateTime = DateTime.UtcNow;
                    await _userRepository.SaveChanges(CancellationToken.None);
                    activity?.SetStatus(ActivityStatusCode.Ok, "User's group is added");
                    await _busControl.Publish(new AssignUserGroupSuccessEvent
                    {
                        Realm = prefix,
                        Id = id
                    });
                    return new ContentResult
                    {
                        Content = JsonSerializer.Serialize(newGroup),
                        ContentType = "application/json",
                        StatusCode = (int)HttpStatusCode.Created
                    };
                }
                catch (OAuthException ex)
                {
                    _logger.LogError(ex.ToString());
                    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                    await _busControl.Publish(new AssignUserGroupFailureEvent
                    {
                        Realm = prefix,
                        Id = id
                    });
                    return BuildError(ex);
                }
            }
        }

        [HttpDelete]
        public async Task<IActionResult> RemoveGroup([FromRoute] string prefix, string id, string groupId)
        {
            prefix = prefix ?? Constants.DefaultRealm;
            using (var activity = Tracing.IdServerActivitySource.StartActivity("Remove user's group"))
            {
                try
                {
                    CheckAccessToken(prefix, Constants.StandardScopes.Users.Name, _jwtBuilder);
                    var user = await _userRepository
                        .Query()
                        .Include(u => u.Groups)
                        .Include(u => u.Realms)
                        .SingleOrDefaultAsync(a => a.Id == id && a.Realms.Any(r => r.RealmsName == prefix));
                    if (user == null) throw new OAuthException(HttpStatusCode.NotFound, ErrorCodes.NOT_FOUND, string.Format(ErrorMessages.UNKNOWN_USER, id));
                    var assignedGroup = user.Groups.SingleOrDefault(g => g.Id == groupId);
                    if (assignedGroup == null) throw new OAuthException(HttpStatusCode.BadRequest, ErrorCodes.INVALID_REQUEST, string.Format(ErrorMessages.UNKNOWN_USER_GROUP, groupId));
                    user.Groups.Remove(assignedGroup);
                    user.UpdateDateTime = DateTime.UtcNow;
                    await _userRepository.SaveChanges(CancellationToken.None);
                    activity?.SetStatus(ActivityStatusCode.Ok, "User's group is removed");
                    await _busControl.Publish(new RemoveUserGroupSuccessEvent
                    {
                        Realm = prefix,
                        Id = id
                    });
                    return new NoContentResult();
                }
                catch (OAuthException ex)
                {
                    _logger.LogError(ex.ToString());
                    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                    await _busControl.Publish(new RemoveUserGroupFailureEvent
                    {
                        Realm = prefix,
                        Id = id
                    });
                    return BuildError(ex);
                }
            }
        }

        #endregion

        #region Consents

        [HttpDelete]
        public async Task<IActionResult> RevokeConsent([FromRoute] string prefix, string id, string consentId)
        {
            prefix = prefix ?? Constants.DefaultRealm;
            using (var activity = Tracing.IdServerActivitySource.StartActivity("Revoke user's consent"))
            {
                try
                {
                    CheckAccessToken(prefix, Constants.StandardScopes.Users.Name, _jwtBuilder);
                    var user = await _userRepository
                        .Query()
                        .Include(u => u.Consents)
                        .Include(u => u.Realms)
                        .SingleOrDefaultAsync(a => a.Id == id && a.Realms.Any(r => r.RealmsName == prefix));
                    if (user == null) throw new OAuthException(HttpStatusCode.NotFound, ErrorCodes.NOT_FOUND, string.Format(ErrorMessages.UNKNOWN_USER, id));
                    var consent = user.Consents.SingleOrDefault(c => c.Id == consentId);
                    if (consent == null) throw new OAuthException(HttpStatusCode.BadRequest, ErrorCodes.INVALID_REQUEST, string.Format(ErrorMessages.UNKNOWN_USER_CONSENT, consentId));
                    user.Consents.Remove(consent);
                    user.UpdateDateTime = DateTime.UtcNow;
                    await _userRepository.SaveChanges(CancellationToken.None);
                    activity?.SetStatus(ActivityStatusCode.Ok, "User's consent is revoked");
                    await _busControl.Publish(new RevokeUserConsentSuccessEvent
                    {
                        Realm = prefix,
                        Id = id
                    });
                    return new NoContentResult();
                }
                catch (OAuthException ex)
                {
                    _logger.LogError(ex.ToString());
                    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                    await _busControl.Publish(new RevokeUserConsentFailureEvent
                    {
                        Realm = prefix,
                        Id = id
                    });
                    return BuildError(ex);
                }
            }
        }

        #endregion

        #region Sessions

        [HttpDelete]
        public async Task<IActionResult> RevokeSession([FromRoute] string prefix, string id, string sessionId)
        {
            prefix = prefix ?? Constants.DefaultRealm;
            using (var activity = Tracing.IdServerActivitySource.StartActivity("Revoke user's session"))
            {
                try
                {
                    CheckAccessToken(prefix, Constants.StandardScopes.Users.Name, _jwtBuilder);
                    var user = await _userRepository
                        .Query()
                        .Include(u => u.Sessions)
                        .Include(u => u.Realms)
                        .SingleOrDefaultAsync(a => a.Id == id && a.Realms.Any(r => r.RealmsName == prefix));
                    if (user == null) throw new OAuthException(HttpStatusCode.NotFound, ErrorCodes.NOT_FOUND, string.Format(ErrorMessages.UNKNOWN_USER, id));
                    var session = user.Sessions.SingleOrDefault(c => c.SessionId == sessionId);
                    if (session == null) throw new OAuthException(HttpStatusCode.BadRequest, ErrorCodes.INVALID_REQUEST, string.Format(ErrorMessages.UNKNOWN_USER_SESSION, sessionId));
                    session.State = UserSessionStates.Rejected;
                    user.UpdateDateTime = DateTime.UtcNow;
                    await _userRepository.SaveChanges(CancellationToken.None);
                    activity?.SetStatus(ActivityStatusCode.Ok, "User's session is revoked");
                    await _busControl.Publish(new RevokeUserSessionSuccessEvent
                    {
                        Realm = prefix,
                        Id = id
                    });
                    return new NoContentResult();
                }
                catch (OAuthException ex)
                {
                    _logger.LogError(ex.ToString());
                    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                    await _busControl.Publish(new RevokeUserSessionFailureEvent
                    {
                        Realm = prefix,
                        Id = id
                    });
                    return BuildError(ex);
                }
            }
        }

        #endregion

        #region External Auth Providers

        [HttpPost]
        public async Task<IActionResult> UnlinkExternalAuthProvider([FromRoute] string prefix, string id, [FromBody] UnlinkExternalAuthProviderRequest request)
        {
            prefix = prefix ?? Constants.DefaultRealm;
            using (var activity = Tracing.IdServerActivitySource.StartActivity("Unlink user's external authentication provider"))
            {
                try
                {
                    CheckAccessToken(prefix, Constants.StandardScopes.Users.Name, _jwtBuilder);
                    if (request == null) throw new OAuthException(HttpStatusCode.BadRequest, ErrorCodes.INVALID_REQUEST, ErrorMessages.INVALID_INCOMING_REQUEST);
                    if (string.IsNullOrWhiteSpace(request.Scheme)) throw new OAuthException(HttpStatusCode.BadRequest, ErrorCodes.INVALID_REQUEST, string.Format(ErrorMessages.MISSING_PARAMETER, UserExternalAuthProviderNames.Scheme));
                    if (string.IsNullOrWhiteSpace(request.Subject)) throw new OAuthException(HttpStatusCode.BadRequest, ErrorCodes.INVALID_REQUEST, string.Format(ErrorMessages.MISSING_PARAMETER, UserExternalAuthProviderNames.Subject));
                    var user = await _userRepository
                        .Query()
                        .Include(u => u.ExternalAuthProviders)
                        .Include(u => u.Realms)
                        .SingleOrDefaultAsync(a => a.Id == id && a.Realms.Any(r => r.RealmsName == prefix));
                    if (user == null) throw new OAuthException(HttpStatusCode.NotFound, ErrorCodes.NOT_FOUND, string.Format(ErrorMessages.UNKNOWN_USER, id));
                    var externalAuthProvider = user.ExternalAuthProviders.SingleOrDefault(c => c.Subject == request.Subject && c.Scheme == request.Scheme);
                    if (externalAuthProvider == null) throw new OAuthException(HttpStatusCode.BadRequest, ErrorCodes.INVALID_REQUEST, ErrorMessages.UNKNOWN_USER_EXTERNALAUTHPROVIDER);
                    user.ExternalAuthProviders.Remove(externalAuthProvider);
                    user.UpdateDateTime = DateTime.UtcNow;
                    await _userRepository.SaveChanges(CancellationToken.None);
                    activity?.SetStatus(ActivityStatusCode.Ok, "User's external authentication provider is unlinked");
                    await _busControl.Publish(new UnlinkUserExternalAuthProviderSuccessEvent
                    {
                        Realm = prefix,
                        Id = id
                    });
                    return new NoContentResult();
                }
                catch (OAuthException ex)
                {
                    _logger.LogError(ex.ToString());
                    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                    await _busControl.Publish(new UnlinkUserExternalAuthProviderFailureEvent
                    {
                        Realm = prefix,
                        Id = id
                    });
                    return BuildError(ex);
                }
            }
        }

        #endregion

        #region DID

        [HttpPost]
        public async Task<IActionResult> GenerateDecentralizedIdentity([FromRoute] string prefix, string id, [FromBody] GenerateDecentralizedIdentityRequest request, CancellationToken cancellationToken)
        {
            using (var activity = Tracing.IdServerActivitySource.StartActivity("Generate decentralized identity"))
            {
                try
                {
                    prefix = prefix ?? Constants.DefaultRealm;
                    CheckAccessToken(prefix, Constants.StandardScopes.Users.Name, _jwtBuilder);
                    Validate();
                    var user = await _userRepository.Get(us => us.Include(u => u.Realms).Include(u => u.CredentialOffers).FirstOrDefaultAsync(u => u.Id == id && u.Realms.Any(r => r.RealmsName == prefix), cancellationToken));
                    if (user == null) return BuildError(HttpStatusCode.NotFound, ErrorCodes.INVALID_REQUEST, string.Format(ErrorMessages.UNKNOWN_USER, id));
                    var generator = _generators.FirstOrDefault(g => g.Method == request.Method);
                    if (generator == null) throw new OAuthException(ErrorCodes.INVALID_REQUEST, string.Format(ErrorMessages.INVALID_DECENTRALIZED_IDENTITY_METHOD, request.Method));
                    var generationResult = await generator.Generate(request.Parameters, cancellationToken);
                    if (!string.IsNullOrWhiteSpace(generationResult.ErrorMessage)) return BuildError(HttpStatusCode.BadRequest, ErrorCodes.INVALID_REQUEST, generationResult.ErrorMessage);
                    user.Did = generationResult.DID;
                    user.DidPrivateHex = generationResult.PrivateKey;
                    user.CredentialOffers.Clear();
                    await _userRepository.SaveChanges(cancellationToken);
                    activity?.SetStatus(ActivityStatusCode.Ok, "Decentralized Identity is generated");
                    return new ContentResult
                    {
                        Content = JsonSerializer.Serialize(new GenerateDecentralizedIdentifierResult
                        {
                            DID = generationResult.DID,
                            PrivateKey = generationResult.PrivateKey
                        }),
                        ContentType = "application/json",
                        StatusCode = (int)HttpStatusCode.Created
                    };
                }
                catch (OAuthException ex)
                {
                    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                    return BuildError(ex);
                }
            }

            void Validate()
            {
                if (request == null) throw new OAuthException(ErrorCodes.INVALID_REQUEST, ErrorMessages.INVALID_INCOMING_REQUEST);
                if (string.IsNullOrWhiteSpace(request.Method)) throw new OAuthException(ErrorCodes.INVALID_REQUEST, string.Format(ErrorMessages.MISSING_PARAMETER, GenerateDecentralizedIdentityRequestNames.Method));
            }
        }

        #endregion
    }
}
