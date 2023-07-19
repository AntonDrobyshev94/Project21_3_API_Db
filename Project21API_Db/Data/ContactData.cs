using Project21API_Db.ContextFolder;
using Project21API_Db.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Project21API_Db.AuthContactApp;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Project21API_Db.Data
{
    public class ContactData
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly DataContext context;
        public ContactData(DataContext context, UserManager<User> userManager,
                                SignInManager<User> signInManager,
                                RoleManager<IdentityRole> roleManager)
        {
            this.context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        public void AddContacts(Contact contact)
        {
            using (var context = new DataContext())
            {
                context.Contacts.Add(contact);
                context.SaveChanges();
            }
        }

        public IEnumerable<IContact> GetContacts()
        {
            return this.context.Contacts;
        }

        public async void DeleteContact(int id)
        {
            using (var context = new DataContext())
            {
                Contact contact = await context.Contacts.FirstOrDefaultAsync(x => x.ID == id);
                if (contact != null)
                {
                    context.Contacts.Remove(contact);
                    await context.SaveChangesAsync();
                }
            }
        }
        public async Task<IContact> GetContactByID(int Id) => ((IContact)await context.Contacts.FirstOrDefaultAsync(x => x.ID == Id));

        public async void ChangeContact(int id, Contact contact)
        {
            using (var context = new DataContext())
            {
                Contact concreteContact = await context.Contacts.FirstOrDefaultAsync(x => x.ID == id);
                concreteContact.Name = contact.Name;
                concreteContact.Surname = contact.Surname;
                concreteContact.FatherName = contact.FatherName;
                concreteContact.TelephoneNumber = contact.TelephoneNumber;
                concreteContact.ResidenceAdress = contact.ResidenceAdress;
                concreteContact.Description = contact.Description;
                await context.SaveChangesAsync();
            }
        }

        public async Task<Contact> Details(int id)
        {
            IContact concreteContact = await GetContactByID(id);
            return (Contact)concreteContact;
        }

        [HttpGet, ValidateAntiForgeryToken]
        public async Task<bool> Login(string loginProp, string password)
        {
            if (loginProp != null)
            {
                var loginResult = await _signInManager.PasswordSignInAsync(loginProp,
                password,
                false,
                lockoutOnFailure: false);

                if (loginResult.Succeeded)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
