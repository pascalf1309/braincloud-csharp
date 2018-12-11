using NUnit.Core;
using NUnit.Framework;
using BrainCloud;
using System.Collections.Generic;

namespace BrainCloudTests
{
    [TestFixture]
    public class TestAuthenticate : TestFixtureNoAuth
    {
        [TearDown]
        public void Cleanup()
        {
            _bc.Client.OverrideCountryCode(null);
            _bc.Client.OverrideLanguageCode(null);
        }

        [Test]
        public void TestAuthenticateUniversal()
        {
            TestResult tr = new TestResult(_bc);

            _bc.Client.AuthenticationService.AuthenticateUniversal(
                GetUser(Users.UserA).Id,
                GetUser(Users.UserA).Password,
                true,
                tr.ApiSuccess, tr.ApiError);

            tr.Run();
        }

        [Test]
        public void TestAuthenticateHandoff()
        {
            TestResult tr = new TestResult(_bc);

            _bc.Client.AuthenticationService.AuthenticateHandoff(
                "invalid_handOffId",
                "invalid_securityToken",
                tr.ApiSuccess, tr.ApiError);

            //expect token to not match user
            tr.RunExpectFail(403, ReasonCodes.TOKEN_DOES_NOT_MATCH_USER);
        }   

        /*[Test]
        public void TestAuthenticateAnonymous()
        {
            TestResult tr = new TestResult(_bc);

            _bc.AuthenticationService.AuthenticateAnonymous(
                true,
                tr.ApiSuccess, tr.ApiError);

            tr.Run();
        }*/

        [Test]
        public void TestAuthenticateEmailPassword()
        {
            TestResult tr = new TestResult(_bc);

            _bc.Client.AuthenticationService.AuthenticateEmailPassword(
                GetUser(Users.UserA).Email,
                GetUser(Users.UserA).Password,
                true,
                tr.ApiSuccess, tr.ApiError);

            tr.Run();
        }

        [Test]
        public void TestResetEmailPassword()
        {
            TestResult tr = new TestResult(_bc);

            string email = "braincloudunittest@gmail.com";

            _bc.Client.AuthenticationService.ResetEmailPassword(
                email,
                tr.ApiSuccess, tr.ApiError);

            tr.Run();
        }
        
        [Test]
        public void TestResetEmailPasswordAdvanced()
        {
            TestResult tr = new TestResult(_bc);

            string email = "braincloudunittest@gmail.com";
            //string email2 = GetUser(Users.UserA).Email;

            Dictionary<string, object> testJson = new Dictionary<string, object>();
            testJson.Add("fromAddress", "fromAddress");
            testJson.Add("fromName", "fromName");
            testJson.Add("replyName", "replyName");
            testJson.Add("templateId", "8f14c77d-61f4-4966-ab6d-0bee8b13d090");
            testJson.Add("subject", "subject");
            testJson.Add("body", "body here");
            Dictionary<string, object> substitutions = new Dictionary<string, object>();
            substitutions.Add(":name", "John Doe");
            substitutions.Add(":resetLink", "www.dummyLink.io");
            testJson.Add("substitutions", substitutions);
            string[] categories = new string[2];
            categories[0] = "category1";
            categories[1] = "category2";
            testJson.Add("categories", categories);

            //create a session
            _bc.Client.AuthenticationService.AuthenticateEmailPassword(
            GetUser(Users.UserA).Email,
            GetUser(Users.UserA).Password,
            true,
            tr.ApiSuccess, tr.ApiError);
            tr.Run();
        

            _bc.Client.AuthenticationService.ResetEmailPasswordAdvanced(
                email,
                testJson,
                tr.ApiSuccess, tr.ApiError);
            tr.RunExpectFail(StatusCodes.BAD_REQUEST, ReasonCodes.INVALID_FROM_ADDRESS);
        }

        [Test]
        public void TestAuthenticateWithHeartbeat()
        {
            TestResult tr = new TestResult(_bc);

            // Insert heartbeat as first packet. This would normally cause the
            // server to reject the second authenticate packet but with the
            // new comms change, this should result in the heartbeat being
            // removed from the message bundle.
            _bc.Client.SendHeartbeat();

            _bc.Client.AuthenticationService.AuthenticateEmailPassword(
                GetUser(Users.UserA).Email,
                GetUser(Users.UserA).Password,
                true,
                tr.ApiSuccess, tr.ApiError);

            tr.Run();
        }

        [Test]
        public void TestSetCountryCode()
        {
            TestResult tr = new TestResult(_bc);

            string countryCode = "RU";

            _bc.Client.OverrideCountryCode(countryCode);

            _bc.Client.AuthenticationService.AuthenticateUniversal(
                GetUser(Users.UserA).Id,
                GetUser(Users.UserA).Password,
                true,
                tr.ApiSuccess, tr.ApiError);

            tr.Run();

            var data = tr.m_response["data"] as Dictionary<string, object>;
            string code = data["countryCode"] as string;
            Assert.AreEqual(countryCode, code);
        }

        [Test]
        public void TestSetLanguageCode()
        {
            TestResult tr = new TestResult(_bc);

            string languageCode = "ru";

            _bc.Client.OverrideLanguageCode(languageCode);

            _bc.Client.AuthenticationService.AuthenticateUniversal(
                GetUser(Users.UserA).Id,
                GetUser(Users.UserA).Password,
                true,
                tr.ApiSuccess, tr.ApiError);

            tr.Run();

            var data = tr.m_response["data"] as Dictionary<string, object>;
            string code = data["languageCode"] as string;
            Assert.AreEqual(languageCode, code);
        }

        [Test]
        public void TestAuthFirst()
        {
            TestResult tr = new TestResult(_bc);

            _bc.PlayerStateService.ReadUserState(tr.ApiSuccess, tr.ApiError);

            _bc.Client.AuthenticationService.AuthenticateEmailPassword(
                GetUser(Users.UserA).Email,
                GetUser(Users.UserA).Password,
                true,
                tr.ApiSuccess, tr.ApiError);

            tr.Run();
            tr.Run();
        }
    }
}