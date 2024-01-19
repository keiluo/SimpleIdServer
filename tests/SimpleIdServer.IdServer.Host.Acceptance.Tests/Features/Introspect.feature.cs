﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by SpecFlow (https://www.specflow.org/).
//      SpecFlow Version:3.9.0.0
//      SpecFlow Generator Version:3.9.0.0
// 
//      Changes to this file may cause incorrect behavior and will be lost if
//      the code is regenerated.
//  </auto-generated>
// ------------------------------------------------------------------------------
#region Designer generated code
#pragma warning disable
namespace SimpleIdServer.IdServer.Host.Acceptance.Tests.Features
{
    using TechTalk.SpecFlow;
    using System;
    using System.Linq;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "3.9.0.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public partial class IntrospectFeature : object, Xunit.IClassFixture<IntrospectFeature.FixtureData>, System.IDisposable
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
        private static string[] featureTags = ((string[])(null));
        
        private Xunit.Abstractions.ITestOutputHelper _testOutputHelper;
        
#line 1 "Introspect.feature"
#line hidden
        
        public IntrospectFeature(IntrospectFeature.FixtureData fixtureData, SimpleIdServer_IdServer_Host_Acceptance_Tests_XUnitAssemblyFixture assemblyFixture, Xunit.Abstractions.ITestOutputHelper testOutputHelper)
        {
            this._testOutputHelper = testOutputHelper;
            this.TestInitialize();
        }
        
        public static void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Features", "Introspect", "\tCheck result returned during the token introspection", ProgrammingLanguage.CSharp, featureTags);
            testRunner.OnFeatureStart(featureInfo);
        }
        
        public static void FeatureTearDown()
        {
            testRunner.OnFeatureEnd();
            testRunner = null;
        }
        
        public void TestInitialize()
        {
        }
        
        public void TestTearDown()
        {
            testRunner.OnScenarioEnd();
        }
        
        public void ScenarioInitialize(TechTalk.SpecFlow.ScenarioInfo scenarioInfo)
        {
            testRunner.OnScenarioInitialize(scenarioInfo);
            testRunner.ScenarioContext.ScenarioContainer.RegisterInstanceAs<Xunit.Abstractions.ITestOutputHelper>(_testOutputHelper);
        }
        
        public void ScenarioStart()
        {
            testRunner.OnScenarioStart();
        }
        
        public void ScenarioCleanup()
        {
            testRunner.CollectScenarioErrors();
        }
        
        void System.IDisposable.Dispose()
        {
            this.TestTearDown();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="IsActive is false when the token doesn\'t exist")]
        [Xunit.TraitAttribute("FeatureTitle", "Introspect")]
        [Xunit.TraitAttribute("Description", "IsActive is false when the token doesn\'t exist")]
        public void IsActiveIsFalseWhenTheTokenDoesntExist()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("IsActive is false when the token doesn\'t exist", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 4
this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((TagHelper.ContainsIgnoreTag(tagsOfScenario) || TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                this.ScenarioStart();
                TechTalk.SpecFlow.Table table448 = new TechTalk.SpecFlow.Table(new string[] {
                            "Key",
                            "Value"});
                table448.AddRow(new string[] {
                            "client_id",
                            "firstClient"});
                table448.AddRow(new string[] {
                            "client_secret",
                            "password"});
                table448.AddRow(new string[] {
                            "token",
                            "token"});
#line 5
 testRunner.When("execute HTTP POST request \'https://localhost:8080/token_info\'", ((string)(null)), table448, "When ");
#line hidden
#line 11
 testRunner.And("extract JSON from body", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 13
 testRunner.Then("HTTP status code equals to \'200\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
#line 14
 testRunner.And("JSON \'$.active\'=\'false\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="IsActive is false when the token is expired")]
        [Xunit.TraitAttribute("FeatureTitle", "Introspect")]
        [Xunit.TraitAttribute("Description", "IsActive is false when the token is expired")]
        public void IsActiveIsFalseWhenTheTokenIsExpired()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("IsActive is false when the token is expired", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 16
this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((TagHelper.ContainsIgnoreTag(tagsOfScenario) || TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                this.ScenarioStart();
                TechTalk.SpecFlow.Table table449 = new TechTalk.SpecFlow.Table(new string[] {
                            "Key",
                            "Value"});
                table449.AddRow(new string[] {
                            "client_id",
                            "thirteenClient"});
                table449.AddRow(new string[] {
                            "client_secret",
                            "password"});
                table449.AddRow(new string[] {
                            "scope",
                            "secondScope"});
                table449.AddRow(new string[] {
                            "grant_type",
                            "client_credentials"});
#line 17
 testRunner.When("execute HTTP POST request \'https://localhost:8080/token\'", ((string)(null)), table449, "When ");
#line hidden
#line 24
 testRunner.And("extract JSON from body", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 25
 testRunner.And("extract parameter \'$.access_token\' from JSON body into \'accessToken\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
                TechTalk.SpecFlow.Table table450 = new TechTalk.SpecFlow.Table(new string[] {
                            "Key",
                            "Value"});
                table450.AddRow(new string[] {
                            "client_id",
                            "thirteenClient"});
                table450.AddRow(new string[] {
                            "client_secret",
                            "password"});
                table450.AddRow(new string[] {
                            "token",
                            "$accessToken$"});
#line 27
 testRunner.And("execute HTTP POST request \'https://localhost:8080/token_info\'", ((string)(null)), table450, "And ");
#line hidden
#line 33
 testRunner.And("extract JSON from body", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 35
 testRunner.Then("HTTP status code equals to \'200\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
#line 36
 testRunner.And("JSON \'$.active\'=\'false\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="IsActive is true when the token is introspected")]
        [Xunit.TraitAttribute("FeatureTitle", "Introspect")]
        [Xunit.TraitAttribute("Description", "IsActive is true when the token is introspected")]
        public void IsActiveIsTrueWhenTheTokenIsIntrospected()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("IsActive is true when the token is introspected", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 38
this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((TagHelper.ContainsIgnoreTag(tagsOfScenario) || TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                this.ScenarioStart();
                TechTalk.SpecFlow.Table table451 = new TechTalk.SpecFlow.Table(new string[] {
                            "Key",
                            "Value"});
                table451.AddRow(new string[] {
                            "client_id",
                            "firstClient"});
                table451.AddRow(new string[] {
                            "client_secret",
                            "password"});
                table451.AddRow(new string[] {
                            "scope",
                            "firstScope"});
                table451.AddRow(new string[] {
                            "grant_type",
                            "client_credentials"});
#line 39
 testRunner.When("execute HTTP POST request \'https://localhost:8080/token\'", ((string)(null)), table451, "When ");
#line hidden
#line 46
 testRunner.And("extract JSON from body", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 47
 testRunner.And("extract parameter \'$.access_token\' from JSON body into \'accessToken\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
                TechTalk.SpecFlow.Table table452 = new TechTalk.SpecFlow.Table(new string[] {
                            "Key",
                            "Value"});
                table452.AddRow(new string[] {
                            "client_id",
                            "firstClient"});
                table452.AddRow(new string[] {
                            "client_secret",
                            "password"});
                table452.AddRow(new string[] {
                            "token",
                            "$accessToken$"});
#line 49
 testRunner.And("execute HTTP POST request \'https://localhost:8080/token_info\'", ((string)(null)), table452, "And ");
#line hidden
#line 55
 testRunner.And("extract JSON from body", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 57
 testRunner.Then("HTTP status code equals to \'200\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
#line 58
 testRunner.And("JSON \'$.active\'=\'true\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 59
 testRunner.And("JSON \'$.client_id\'=\'firstClient\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 60
 testRunner.And("JSON \'$.scope\'=\'firstScope\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 61
 testRunner.And("JSON \'$.iss\'=\'https://localhost:8080\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="JKT is returned")]
        [Xunit.TraitAttribute("FeatureTitle", "Introspect")]
        [Xunit.TraitAttribute("Description", "JKT is returned")]
        public void JKTIsReturned()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("JKT is returned", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 63
this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((TagHelper.ContainsIgnoreTag(tagsOfScenario) || TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                this.ScenarioStart();
                TechTalk.SpecFlow.Table table453 = new TechTalk.SpecFlow.Table(new string[] {
                            "Key",
                            "Value"});
                table453.AddRow(new string[] {
                            "htm",
                            "POST"});
                table453.AddRow(new string[] {
                            "htu",
                            "https://localhost:8080/token"});
#line 64
 testRunner.When("build DPoP proof", ((string)(null)), table453, "When ");
#line hidden
                TechTalk.SpecFlow.Table table454 = new TechTalk.SpecFlow.Table(new string[] {
                            "Key",
                            "Value"});
                table454.AddRow(new string[] {
                            "client_id",
                            "sixtyThreeClient"});
                table454.AddRow(new string[] {
                            "client_secret",
                            "password"});
                table454.AddRow(new string[] {
                            "scope",
                            "firstScope"});
                table454.AddRow(new string[] {
                            "grant_type",
                            "client_credentials"});
                table454.AddRow(new string[] {
                            "DPoP",
                            "$DPOP$"});
#line 69
 testRunner.And("execute HTTP POST request \'https://localhost:8080/token\'", ((string)(null)), table454, "And ");
#line hidden
#line 77
 testRunner.And("extract JSON from body", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 78
 testRunner.And("extract parameter \'$.access_token\' from JSON body into \'accessToken\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
                TechTalk.SpecFlow.Table table455 = new TechTalk.SpecFlow.Table(new string[] {
                            "Key",
                            "Value"});
                table455.AddRow(new string[] {
                            "client_id",
                            "sixtyThreeClient"});
                table455.AddRow(new string[] {
                            "client_secret",
                            "password"});
                table455.AddRow(new string[] {
                            "token",
                            "$accessToken$"});
#line 80
 testRunner.And("execute HTTP POST request \'https://localhost:8080/token_info\'", ((string)(null)), table455, "And ");
#line hidden
#line 86
 testRunner.And("extract JSON from body", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 88
 testRunner.Then("HTTP status code equals to \'200\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
#line 89
 testRunner.And("JSON \'$.active\'=\'true\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 90
 testRunner.And("JSON \'$.client_id\'=\'sixtyThreeClient\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 91
 testRunner.And("JSON \'$.scope\'=\'firstScope\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 92
 testRunner.And("JSON \'$.iss\'=\'https://localhost:8080\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 93
 testRunner.And("JSON exists \'$.cnf.jkt\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="Introspect a reference access token")]
        [Xunit.TraitAttribute("FeatureTitle", "Introspect")]
        [Xunit.TraitAttribute("Description", "Introspect a reference access token")]
        public void IntrospectAReferenceAccessToken()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Introspect a reference access token", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 95
this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((TagHelper.ContainsIgnoreTag(tagsOfScenario) || TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                this.ScenarioStart();
                TechTalk.SpecFlow.Table table456 = new TechTalk.SpecFlow.Table(new string[] {
                            "Key",
                            "Value"});
                table456.AddRow(new string[] {
                            "client_id",
                            "sixtyEightClient"});
                table456.AddRow(new string[] {
                            "client_secret",
                            "password"});
                table456.AddRow(new string[] {
                            "scope",
                            "firstScope"});
                table456.AddRow(new string[] {
                            "grant_type",
                            "client_credentials"});
#line 96
 testRunner.When("execute HTTP POST request \'https://localhost:8080/token\'", ((string)(null)), table456, "When ");
#line hidden
#line 103
 testRunner.And("extract JSON from body", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 104
 testRunner.And("extract parameter \'$.access_token\' from JSON body into \'accessToken\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
                TechTalk.SpecFlow.Table table457 = new TechTalk.SpecFlow.Table(new string[] {
                            "Key",
                            "Value"});
                table457.AddRow(new string[] {
                            "client_id",
                            "sixtyEightClient"});
                table457.AddRow(new string[] {
                            "client_secret",
                            "password"});
                table457.AddRow(new string[] {
                            "token",
                            "$accessToken$"});
#line 106
 testRunner.And("execute HTTP POST request \'https://localhost:8080/token_info\'", ((string)(null)), table457, "And ");
#line hidden
#line 112
 testRunner.And("extract JSON from body", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 114
 testRunner.Then("HTTP status code equals to \'200\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
#line 115
 testRunner.And("JSON \'$.active\'=\'true\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 116
 testRunner.And("JSON \'$.client_id\'=\'sixtyEightClient\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 117
 testRunner.And("JSON \'$.scope\'=\'firstScope\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "3.9.0.0")]
        [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
        public class FixtureData : System.IDisposable
        {
            
            public FixtureData()
            {
                IntrospectFeature.FeatureSetup();
            }
            
            void System.IDisposable.Dispose()
            {
                IntrospectFeature.FeatureTearDown();
            }
        }
    }
}
#pragma warning restore
#endregion
