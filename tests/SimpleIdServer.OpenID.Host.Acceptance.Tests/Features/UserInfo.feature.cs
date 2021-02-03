// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by SpecFlow (http://www.specflow.org/).
//      SpecFlow Version:3.0.0.0
//      SpecFlow Generator Version:3.0.0.0
// 
//      Changes to this file may cause incorrect behavior and will be lost if
//      the code is regenerated.
//  </auto-generated>
// ------------------------------------------------------------------------------
#region Designer generated code
#pragma warning disable
namespace SimpleIdServer.OpenID.Host.Acceptance.Tests.Features
{
    using TechTalk.SpecFlow;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "3.0.0.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public partial class UserInfoFeature : Xunit.IClassFixture<UserInfoFeature.FixtureData>, System.IDisposable
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
        private Xunit.Abstractions.ITestOutputHelper _testOutputHelper;
        
#line 1 "UserInfo.feature"
#line hidden
        
        public UserInfoFeature(UserInfoFeature.FixtureData fixtureData, Xunit.Abstractions.ITestOutputHelper testOutputHelper)
        {
            this._testOutputHelper = testOutputHelper;
            this.TestInitialize();
        }
        
        public static void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "UserInfo", "\tCheck the userinfo endpoint", ProgrammingLanguage.CSharp, ((string[])(null)));
            testRunner.OnFeatureStart(featureInfo);
        }
        
        public static void FeatureTearDown()
        {
            testRunner.OnFeatureEnd();
            testRunner = null;
        }
        
        public virtual void TestInitialize()
        {
        }
        
        public virtual void ScenarioTearDown()
        {
            testRunner.OnScenarioEnd();
        }
        
        public virtual void ScenarioInitialize(TechTalk.SpecFlow.ScenarioInfo scenarioInfo)
        {
            testRunner.OnScenarioInitialize(scenarioInfo);
            testRunner.ScenarioContext.ScenarioContainer.RegisterInstanceAs<Xunit.Abstractions.ITestOutputHelper>(_testOutputHelper);
        }
        
        public virtual void ScenarioStart()
        {
            testRunner.OnScenarioStart();
        }
        
        public virtual void ScenarioCleanup()
        {
            testRunner.CollectScenarioErrors();
        }
        
        void System.IDisposable.Dispose()
        {
            this.ScenarioTearDown();
        }
        
        [Xunit.FactAttribute(DisplayName="Check correct types are returned by user info")]
        [Xunit.TraitAttribute("FeatureTitle", "UserInfo")]
        [Xunit.TraitAttribute("Description", "Check correct types are returned by user info")]
        public virtual void CheckCorrectTypesAreReturnedByUserInfo()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Check correct types are returned by user info", null, ((string[])(null)));
#line 4
this.ScenarioInitialize(scenarioInfo);
            this.ScenarioStart();
#line hidden
            TechTalk.SpecFlow.Table table198 = new TechTalk.SpecFlow.Table(new string[] {
                        "Key",
                        "Value"});
            table198.AddRow(new string[] {
                        "redirect_uris",
                        "[https://web.com]"});
            table198.AddRow(new string[] {
                        "scope",
                        "profile email address"});
#line 5
 testRunner.When("execute HTTP POST JSON request \'http://localhost/register\'", ((string)(null)), table198, "When ");
#line 10
 testRunner.And("extract JSON from body", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 11
 testRunner.And("extract parameter \'client_id\' from JSON body", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table199 = new TechTalk.SpecFlow.Table(new string[] {
                        "Type",
                        "Kid",
                        "AlgName"});
            table199.AddRow(new string[] {
                        "SIG",
                        "1",
                        "ES256"});
#line 13
 testRunner.And("add JSON web key to Authorization Server and store into \'jwks\'", ((string)(null)), table199, "And ");
#line hidden
            TechTalk.SpecFlow.Table table200 = new TechTalk.SpecFlow.Table(new string[] {
                        "Key",
                        "Value"});
            table200.AddRow(new string[] {
                        "sub",
                        "administrator"});
            table200.AddRow(new string[] {
                        "aud",
                        "$client_id$"});
            table200.AddRow(new string[] {
                        "scope",
                        "[openid,profile,email,address]"});
#line 17
 testRunner.And("use \'1\' JWK from \'jwks\' to build JWS and store into \'accesstoken\'", ((string)(null)), table200, "And ");
#line 23
 testRunner.And("add user consent : user=\'administrator\', scope=\'profile email address\', clientId=" +
                    "\'$client_id$\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table201 = new TechTalk.SpecFlow.Table(new string[] {
                        "Key",
                        "Value"});
            table201.AddRow(new string[] {
                        "access_token",
                        "$accesstoken$"});
#line 25
 testRunner.And("execute HTTP POST request \'http://localhost/userinfo\'", ((string)(null)), table201, "And ");
#line 29
 testRunner.And("extract JSON from body", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 31
 testRunner.Then("HTTP status code equals to \'200\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 32
 testRunner.Then("HTTP header \'Content-Type\' contains \'application/json\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 33
 testRunner.Then("JSON \'name\'=\'Thierry Habart\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 34
 testRunner.Then("JSON \'updated_at\'=1612361922", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 35
 testRunner.Then("JSON \'email_verified\'=true", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 36
 testRunner.Then("JSON \'address.region\'=\'CA\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 37
 testRunner.Then("JSON \'address.street_address\'=\'1234 Hollywood Blvd.\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Xunit.FactAttribute(DisplayName="Check user information are returned when access_token is passed in the HTTP BODY")]
        [Xunit.TraitAttribute("FeatureTitle", "UserInfo")]
        [Xunit.TraitAttribute("Description", "Check user information are returned when access_token is passed in the HTTP BODY")]
        public virtual void CheckUserInformationAreReturnedWhenAccess_TokenIsPassedInTheHTTPBODY()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Check user information are returned when access_token is passed in the HTTP BODY", null, ((string[])(null)));
#line 40
this.ScenarioInitialize(scenarioInfo);
            this.ScenarioStart();
#line hidden
            TechTalk.SpecFlow.Table table202 = new TechTalk.SpecFlow.Table(new string[] {
                        "Key",
                        "Value"});
            table202.AddRow(new string[] {
                        "redirect_uris",
                        "[https://web.com]"});
            table202.AddRow(new string[] {
                        "scope",
                        "profile"});
#line 41
 testRunner.When("execute HTTP POST JSON request \'http://localhost/register\'", ((string)(null)), table202, "When ");
#line 46
 testRunner.And("extract JSON from body", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 47
 testRunner.And("extract parameter \'client_id\' from JSON body", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table203 = new TechTalk.SpecFlow.Table(new string[] {
                        "Type",
                        "Kid",
                        "AlgName"});
            table203.AddRow(new string[] {
                        "SIG",
                        "1",
                        "ES256"});
#line 49
 testRunner.And("add JSON web key to Authorization Server and store into \'jwks\'", ((string)(null)), table203, "And ");
#line hidden
            TechTalk.SpecFlow.Table table204 = new TechTalk.SpecFlow.Table(new string[] {
                        "Key",
                        "Value"});
            table204.AddRow(new string[] {
                        "sub",
                        "administrator"});
            table204.AddRow(new string[] {
                        "aud",
                        "$client_id$"});
            table204.AddRow(new string[] {
                        "scope",
                        "[openid,profile]"});
#line 53
 testRunner.And("use \'1\' JWK from \'jwks\' to build JWS and store into \'accesstoken\'", ((string)(null)), table204, "And ");
#line 59
 testRunner.And("add user consent : user=\'administrator\', scope=\'profile\', clientId=\'$client_id$\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table205 = new TechTalk.SpecFlow.Table(new string[] {
                        "Key",
                        "Value"});
            table205.AddRow(new string[] {
                        "access_token",
                        "$accesstoken$"});
#line 61
 testRunner.And("execute HTTP POST request \'http://localhost/userinfo\'", ((string)(null)), table205, "And ");
#line 65
 testRunner.And("extract JSON from body", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 67
 testRunner.Then("HTTP status code equals to \'200\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 68
 testRunner.Then("HTTP header \'Content-Type\' contains \'application/json\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 69
 testRunner.Then("JSON \'name\'=\'Thierry Habart\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Xunit.FactAttribute(DisplayName="Check user information are returned (JSON)")]
        [Xunit.TraitAttribute("FeatureTitle", "UserInfo")]
        [Xunit.TraitAttribute("Description", "Check user information are returned (JSON)")]
        public virtual void CheckUserInformationAreReturnedJSON()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Check user information are returned (JSON)", null, ((string[])(null)));
#line 71
this.ScenarioInitialize(scenarioInfo);
            this.ScenarioStart();
#line hidden
            TechTalk.SpecFlow.Table table206 = new TechTalk.SpecFlow.Table(new string[] {
                        "Key",
                        "Value"});
            table206.AddRow(new string[] {
                        "redirect_uris",
                        "[https://web.com]"});
            table206.AddRow(new string[] {
                        "scope",
                        "profile"});
#line 72
 testRunner.When("execute HTTP POST JSON request \'http://localhost/register\'", ((string)(null)), table206, "When ");
#line 77
 testRunner.And("extract JSON from body", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 78
 testRunner.And("extract parameter \'client_id\' from JSON body", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table207 = new TechTalk.SpecFlow.Table(new string[] {
                        "Type",
                        "Kid",
                        "AlgName"});
            table207.AddRow(new string[] {
                        "SIG",
                        "1",
                        "ES256"});
#line 80
 testRunner.And("add JSON web key to Authorization Server and store into \'jwks\'", ((string)(null)), table207, "And ");
#line hidden
            TechTalk.SpecFlow.Table table208 = new TechTalk.SpecFlow.Table(new string[] {
                        "Key",
                        "Value"});
            table208.AddRow(new string[] {
                        "sub",
                        "administrator"});
            table208.AddRow(new string[] {
                        "aud",
                        "$client_id$"});
            table208.AddRow(new string[] {
                        "scope",
                        "[openid,profile]"});
#line 84
 testRunner.And("use \'1\' JWK from \'jwks\' to build JWS and store into \'accesstoken\'", ((string)(null)), table208, "And ");
#line 90
 testRunner.And("add user consent : user=\'administrator\', scope=\'profile\', clientId=\'$client_id$\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table209 = new TechTalk.SpecFlow.Table(new string[] {
                        "Key",
                        "Value"});
            table209.AddRow(new string[] {
                        "Authorization",
                        "Bearer $accesstoken$"});
#line 92
 testRunner.And("execute HTTP GET request \'http://localhost/userinfo\'", ((string)(null)), table209, "And ");
#line 96
 testRunner.And("extract JSON from body", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 98
 testRunner.Then("HTTP status code equals to \'200\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 99
 testRunner.Then("HTTP header \'Content-Type\' contains \'application/json\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 100
 testRunner.Then("JSON \'name\'=\'Thierry Habart\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Xunit.FactAttribute(DisplayName="Check user information are returned (JWS)")]
        [Xunit.TraitAttribute("FeatureTitle", "UserInfo")]
        [Xunit.TraitAttribute("Description", "Check user information are returned (JWS)")]
        public virtual void CheckUserInformationAreReturnedJWS()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Check user information are returned (JWS)", null, ((string[])(null)));
#line 102
this.ScenarioInitialize(scenarioInfo);
            this.ScenarioStart();
#line hidden
            TechTalk.SpecFlow.Table table210 = new TechTalk.SpecFlow.Table(new string[] {
                        "Type",
                        "Kid",
                        "AlgName"});
            table210.AddRow(new string[] {
                        "SIG",
                        "1",
                        "ES256"});
#line 103
 testRunner.When("add JSON web key to Authorization Server and store into \'jwks\'", ((string)(null)), table210, "When ");
#line hidden
            TechTalk.SpecFlow.Table table211 = new TechTalk.SpecFlow.Table(new string[] {
                        "Key",
                        "Value"});
            table211.AddRow(new string[] {
                        "redirect_uris",
                        "[https://web.com]"});
            table211.AddRow(new string[] {
                        "scope",
                        "profile"});
            table211.AddRow(new string[] {
                        "userinfo_signed_response_alg",
                        "ES256"});
#line 107
 testRunner.And("execute HTTP POST JSON request \'http://localhost/register\'", ((string)(null)), table211, "And ");
#line 113
 testRunner.And("extract JSON from body", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 114
 testRunner.And("extract parameter \'client_id\' from JSON body", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table212 = new TechTalk.SpecFlow.Table(new string[] {
                        "Key",
                        "Value"});
            table212.AddRow(new string[] {
                        "sub",
                        "administrator"});
            table212.AddRow(new string[] {
                        "aud",
                        "$client_id$"});
            table212.AddRow(new string[] {
                        "scope",
                        "[openid,profile]"});
#line 116
 testRunner.And("use \'1\' JWK from \'jwks\' to build JWS and store into \'accesstoken\'", ((string)(null)), table212, "And ");
#line 122
 testRunner.And("add user consent : user=\'administrator\', scope=\'profile\', clientId=\'$client_id$\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table213 = new TechTalk.SpecFlow.Table(new string[] {
                        "Key",
                        "Value"});
            table213.AddRow(new string[] {
                        "Authorization",
                        "Bearer $accesstoken$"});
#line 124
 testRunner.And("execute HTTP GET request \'http://localhost/userinfo\'", ((string)(null)), table213, "And ");
#line 128
 testRunner.And("extract string from body", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 129
 testRunner.Then("HTTP status code equals to \'200\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 130
 testRunner.Then("HTTP header \'Content-Type\' contains \'application/jwt\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Xunit.FactAttribute(DisplayName="Check user information are returned (JWE)")]
        [Xunit.TraitAttribute("FeatureTitle", "UserInfo")]
        [Xunit.TraitAttribute("Description", "Check user information are returned (JWE)")]
        public virtual void CheckUserInformationAreReturnedJWE()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Check user information are returned (JWE)", null, ((string[])(null)));
#line 132
this.ScenarioInitialize(scenarioInfo);
            this.ScenarioStart();
#line hidden
            TechTalk.SpecFlow.Table table214 = new TechTalk.SpecFlow.Table(new string[] {
                        "Type",
                        "Kid",
                        "AlgName"});
            table214.AddRow(new string[] {
                        "SIG",
                        "1",
                        "ES256"});
#line 133
 testRunner.When("add JSON web key to Authorization Server and store into \'jwks_sig\'", ((string)(null)), table214, "When ");
#line hidden
            TechTalk.SpecFlow.Table table215 = new TechTalk.SpecFlow.Table(new string[] {
                        "Type",
                        "Kid",
                        "AlgName"});
            table215.AddRow(new string[] {
                        "ENC",
                        "2",
                        "RSA1_5"});
#line 137
 testRunner.And("build JSON Web Keys, store JWKS into \'jwks\' and store the public keys into \'jwks_" +
                    "json\'", ((string)(null)), table215, "And ");
#line hidden
            TechTalk.SpecFlow.Table table216 = new TechTalk.SpecFlow.Table(new string[] {
                        "Key",
                        "Value"});
            table216.AddRow(new string[] {
                        "redirect_uris",
                        "[https://web.com]"});
            table216.AddRow(new string[] {
                        "scope",
                        "profile"});
            table216.AddRow(new string[] {
                        "userinfo_signed_response_alg",
                        "ES256"});
            table216.AddRow(new string[] {
                        "userinfo_encrypted_response_alg",
                        "RSA1_5"});
            table216.AddRow(new string[] {
                        "userinfo_encrypted_response_enc",
                        "A128CBC-HS256"});
            table216.AddRow(new string[] {
                        "jwks",
                        "$jwks_json$"});
#line 141
 testRunner.And("execute HTTP POST JSON request \'http://localhost/register\'", ((string)(null)), table216, "And ");
#line 150
 testRunner.And("extract JSON from body", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 151
 testRunner.And("extract parameter \'client_id\' from JSON body", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table217 = new TechTalk.SpecFlow.Table(new string[] {
                        "Key",
                        "Value"});
            table217.AddRow(new string[] {
                        "sub",
                        "administrator"});
            table217.AddRow(new string[] {
                        "aud",
                        "$client_id$"});
            table217.AddRow(new string[] {
                        "scope",
                        "[openid,profile]"});
#line 153
 testRunner.And("use \'1\' JWK from \'jwks_sig\' to build JWS and store into \'accesstoken\'", ((string)(null)), table217, "And ");
#line 159
 testRunner.And("add user consent : user=\'administrator\', scope=\'profile\', clientId=\'$client_id$\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table218 = new TechTalk.SpecFlow.Table(new string[] {
                        "Key",
                        "Value"});
            table218.AddRow(new string[] {
                        "Authorization",
                        "Bearer $accesstoken$"});
#line 161
 testRunner.And("execute HTTP GET request \'http://localhost/userinfo\'", ((string)(null)), table218, "And ");
#line 165
 testRunner.And("extract string from body", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 166
 testRunner.Then("HTTP status code equals to \'200\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 167
 testRunner.Then("HTTP header \'Content-Type\' contains \'application/jwt\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Xunit.FactAttribute(DisplayName="Use claims parameter to get user information from UserInfo endpoint")]
        [Xunit.TraitAttribute("FeatureTitle", "UserInfo")]
        [Xunit.TraitAttribute("Description", "Use claims parameter to get user information from UserInfo endpoint")]
        public virtual void UseClaimsParameterToGetUserInformationFromUserInfoEndpoint()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Use claims parameter to get user information from UserInfo endpoint", null, ((string[])(null)));
#line 169
this.ScenarioInitialize(scenarioInfo);
            this.ScenarioStart();
#line hidden
            TechTalk.SpecFlow.Table table219 = new TechTalk.SpecFlow.Table(new string[] {
                        "Type",
                        "Kid",
                        "AlgName"});
            table219.AddRow(new string[] {
                        "SIG",
                        "1",
                        "ES256"});
#line 170
 testRunner.When("add JSON web key to Authorization Server and store into \'jwks\'", ((string)(null)), table219, "When ");
#line hidden
            TechTalk.SpecFlow.Table table220 = new TechTalk.SpecFlow.Table(new string[] {
                        "Key",
                        "Value"});
            table220.AddRow(new string[] {
                        "redirect_uris",
                        "[https://web.com]"});
            table220.AddRow(new string[] {
                        "scope",
                        "email"});
#line 174
 testRunner.And("execute HTTP POST JSON request \'http://localhost/register\'", ((string)(null)), table220, "And ");
#line 179
 testRunner.And("extract JSON from body", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 180
 testRunner.And("extract parameter \'client_id\' from JSON body", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table221 = new TechTalk.SpecFlow.Table(new string[] {
                        "Key",
                        "Value"});
            table221.AddRow(new string[] {
                        "sub",
                        "administrator"});
            table221.AddRow(new string[] {
                        "aud",
                        "$client_id$"});
            table221.AddRow(new string[] {
                        "claims",
                        "{ userinfo: { name: { essential : true }, email: { essential : true } } }"});
#line 182
 testRunner.And("use \'1\' JWK from \'jwks\' to build JWS and store into \'accesstoken\'", ((string)(null)), table221, "And ");
#line 188
 testRunner.And("add user consent with claim : user=\'administrator\', scope=\'email\', clientId=\'$cli" +
                    "ent_id$\', claim=\'name email\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table222 = new TechTalk.SpecFlow.Table(new string[] {
                        "Key",
                        "Value"});
            table222.AddRow(new string[] {
                        "Authorization",
                        "Bearer $accesstoken$"});
#line 190
 testRunner.And("execute HTTP GET request \'http://localhost/userinfo\'", ((string)(null)), table222, "And ");
#line 194
 testRunner.And("extract JSON from body", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 195
 testRunner.Then("HTTP status code equals to \'200\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 196
 testRunner.Then("HTTP header \'Content-Type\' contains \'application/json\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 197
 testRunner.Then("JSON \'name\'=\'Thierry Habart\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 198
 testRunner.Then("JSON \'email\'=\'habarthierry@hotmail.fr\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Xunit.FactAttribute(DisplayName="Use offline_access scope to get user information from UserInfo endpoint")]
        [Xunit.TraitAttribute("FeatureTitle", "UserInfo")]
        [Xunit.TraitAttribute("Description", "Use offline_access scope to get user information from UserInfo endpoint")]
        public virtual void UseOffline_AccessScopeToGetUserInformationFromUserInfoEndpoint()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Use offline_access scope to get user information from UserInfo endpoint", null, ((string[])(null)));
#line 200
this.ScenarioInitialize(scenarioInfo);
            this.ScenarioStart();
#line hidden
            TechTalk.SpecFlow.Table table223 = new TechTalk.SpecFlow.Table(new string[] {
                        "Type",
                        "Kid",
                        "AlgName"});
            table223.AddRow(new string[] {
                        "SIG",
                        "1",
                        "RS256"});
#line 201
 testRunner.When("add JSON web key to Authorization Server and store into \'jwks\'", ((string)(null)), table223, "When ");
#line hidden
            TechTalk.SpecFlow.Table table224 = new TechTalk.SpecFlow.Table(new string[] {
                        "Key",
                        "Value"});
            table224.AddRow(new string[] {
                        "redirect_uris",
                        "[https://web.com]"});
            table224.AddRow(new string[] {
                        "grant_types",
                        "[authorization_code,refresh_token]"});
            table224.AddRow(new string[] {
                        "response_types",
                        "[code]"});
            table224.AddRow(new string[] {
                        "scope",
                        "offline_access"});
            table224.AddRow(new string[] {
                        "subject_type",
                        "public"});
            table224.AddRow(new string[] {
                        "token_endpoint_auth_method",
                        "client_secret_post"});
#line 205
 testRunner.When("execute HTTP POST JSON request \'http://localhost/register\'", ((string)(null)), table224, "When ");
#line 214
 testRunner.And("extract JSON from body", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 215
 testRunner.And("extract parameter \'client_id\' from JSON body", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 216
 testRunner.And("extract parameter \'client_secret\' from JSON body", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 217
 testRunner.And("add user consent : user=\'administrator\', scope=\'offline_access\', clientId=\'$clien" +
                    "t_id$\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table225 = new TechTalk.SpecFlow.Table(new string[] {
                        "Key",
                        "Value"});
            table225.AddRow(new string[] {
                        "response_type",
                        "code"});
            table225.AddRow(new string[] {
                        "client_id",
                        "$client_id$"});
            table225.AddRow(new string[] {
                        "state",
                        "state"});
            table225.AddRow(new string[] {
                        "response_mode",
                        "query"});
            table225.AddRow(new string[] {
                        "scope",
                        "openid offline_access"});
            table225.AddRow(new string[] {
                        "redirect_uri",
                        "https://web.com"});
            table225.AddRow(new string[] {
                        "ui_locales",
                        "en fr"});
#line 219
 testRunner.And("execute HTTP GET request \'http://localhost/authorization\'", ((string)(null)), table225, "And ");
#line 229
 testRunner.And("extract parameter \'refresh_token\' from redirect url", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table226 = new TechTalk.SpecFlow.Table(new string[] {
                        "Key",
                        "Value"});
            table226.AddRow(new string[] {
                        "client_id",
                        "$client_id$"});
            table226.AddRow(new string[] {
                        "client_secret",
                        "$client_secret$"});
            table226.AddRow(new string[] {
                        "grant_type",
                        "refresh_token"});
            table226.AddRow(new string[] {
                        "refresh_token",
                        "$refresh_token$"});
#line 231
 testRunner.And("execute HTTP POST request \'http://localhost/token\'", ((string)(null)), table226, "And ");
#line 238
 testRunner.And("extract JSON from body", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 239
 testRunner.And("extract parameter \'access_token\' from JSON body", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table227 = new TechTalk.SpecFlow.Table(new string[] {
                        "Key",
                        "Value"});
            table227.AddRow(new string[] {
                        "Authorization",
                        "Bearer $access_token$"});
#line 241
 testRunner.And("execute HTTP GET request \'http://localhost/userinfo\'", ((string)(null)), table227, "And ");
#line 245
 testRunner.And("extract JSON from body", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 247
 testRunner.Then("HTTP status code equals to \'200\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 248
 testRunner.Then("HTTP header \'Content-Type\' contains \'application/json\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 249
 testRunner.Then("JSON \'sub\'=\'administrator\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "3.0.0.0")]
        [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
        public class FixtureData : System.IDisposable
        {
            
            public FixtureData()
            {
                UserInfoFeature.FeatureSetup();
            }
            
            void System.IDisposable.Dispose()
            {
                UserInfoFeature.FeatureTearDown();
            }
        }
    }
}
#pragma warning restore
#endregion
