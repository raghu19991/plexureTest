using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace PlexureAPITest
{
    [TestFixture]
    public class Test
    {
        Service service;

        [OneTimeSetUp]
        public void Setup()
        {
            service = new Service();
        }

        [OneTimeTearDown]
        public void Cleanup()
        {
            if (service != null)
            {
                service.Dispose();
                service = null;
            }
        }

        [Test, Category("Login")]
        public void TEST_001_Login_With_Valid_User_And_Validate_Response()
        {
            var response = service.Login("Tester", "Plexure123");

            response.Expect(HttpStatusCode.OK);
            Assert.AreEqual(1, response.Entity.UserId);
            Assert.AreEqual("Tester", response.Entity.UserName);
            Assert.AreEqual("37cb9e58-99db-423c-9da5-42d5627614c5", response.Entity.AccessToken);
        }


        [Test,Category("Login")]
        public void TEST_002_Login_With_Invalid_User_Details()
        {
            var response = service.Login("Tester", "incorrect");

            response.Expect(HttpStatusCode.Unauthorized);
            response.ExpectError("Error: Unauthorized");
        }

        [Test, Category("Login")]
        public void TEST_003_Login_With_Empty_User_Details()
        {
            var response = service.Login("", "");

            response.Expect(HttpStatusCode.BadRequest);
            response.ExpectError("Error: Username and password required.");
        }

        [Test, Category("Login"),Category("Bug")] //Category 'Bug' is added so that it can be excluded in pipeline/job run and include when fixed or run in another bug pipeline
        public void TEST_004_Login_With_SQL_Injection()
        {
            var response = service.Login("' or '1'='1", "Plexure123");

            response.Expect(HttpStatusCode.Unauthorized);
            response.ExpectError("Error: Unauthorized");
        }

        //POINTS TESTS
        [Test, Category("Points")]
        public void TEST_005_Get_Points_For_Logged_In_User()
        {
            var points = service.GetPoints();
            points.Expect(HttpStatusCode.Accepted);
        }

        //PURCHASE TESTS
        [Test,Category("Purchase")]
        public void TEST_006_Purchase_Product()
        {
            int productId = 1;
            var response = service.Purchase(productId);
            response.Expect(HttpStatusCode.Accepted);
            Assert.AreEqual("Purchase completed.", response.Entity.Message);
            Assert.AreEqual(100, response.Entity.Points);
        }

        [Test, Category("Purchase")]
        public void TEST_007_Invalid_Product_Purchase_Request()
        {
            int productId = 10;
            var response = service.Purchase(productId);
            response.Expect(HttpStatusCode.BadRequest);
            response.ExpectError("Error: Invalid product id");
        }

        [Test,Category("Purchase")]
        public void TEST_008_Check_Increase_In_Points_On_Purchase()
        {
            var existingPoints = service.GetPoints().Entity.Value;
            int productId = 1;
            var PurchaseItem = service.Purchase(productId);
            var pointsForPurchase = PurchaseItem.Entity.Points;
            var currentPoints = service.GetPoints().Entity.Value;
            Assert.AreEqual(existingPoints + pointsForPurchase, currentPoints);

        }
    }
}
