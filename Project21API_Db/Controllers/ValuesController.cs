using Microsoft.AspNetCore.Mvc;
using Project21API_Db.AuthContactApp;
using Project21API_Db.Data;
using Project21API_Db.Models;
using Microsoft.AspNetCore.Authorization;
using System.Data;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace Project21API_Db.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly ContactData repositoryData;

        public ValuesController(ContactData repositoryData)
        {
            this.repositoryData = repositoryData;
        }

        // GET api/values
        [HttpGet]
        public IEnumerable<IContact> Get()
        {
            return repositoryData.GetContacts();
        }

        // GET api/values/5
        [Authorize(Roles = "Admin")]
        [Route("Details/{id}")]
        public async Task<Contact> ContactDetails(int id)
        {
            return await repositoryData.Details(id);
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] Contact value)
        {
            repositoryData.AddContacts(value);
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
            repositoryData.DeleteContact(id);
        }

        [HttpPost]
        [Route("ChangeContactById/{id}")]
        public void ChangeContactById(int id, Contact contact)
        {
            repositoryData.ChangeContact(id, contact);
        }

        [HttpPost]
        [Route("Login/{loginProp}/password/{password}")]
        public async Task<bool> IsLogin(string loginProp, string password)
        {
            return await repositoryData.Login(loginProp, password);
        }
    }
}
