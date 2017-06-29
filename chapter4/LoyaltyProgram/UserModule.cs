using System.Collections.Generic;

using Nancy;
using Nancy.ModelBinding;

namespace LoyaltyProgram
{
    public class UserModule : NancyModule
    {
        private static IDictionary<int, LoyaltyProgramUser> registeredUsers = new Dictionary<int, LoyaltyProgramUser>();

        public UserModule()
            : base("users")
        {
            Post("/", _ =>
            {
                // The request must include a LoyaltyProgramUser in the body. If it doesn't, the request is malformed.
                var newUser = this.Bind<LoyaltyProgramUser>();
                this.AddRegisteredUser(newUser);
                return this.CreatedResponse(newUser);
            });

            Put("/{userId:int}", parameters =>
            {
                int userId = parameters.userId;
                var updatedUser = this.Bind<LoyaltyProgramUser>();

                // Store the updatedUser to a data store
                registeredUsers[userId] = updatedUser;

                return updatedUser; // Nancy turns the user object into a complete response.
            }); 
        }

        private dynamic CreatedResponse(LoyaltyProgramUser newUser)
        {
            return this.Negotiate // Negotiate in an entry point to Nancy's fluent API for creating responses.
                .WithStatusCode(HttpStatusCode.Created) // Uses the 201 Created status code for the response
                .WithHeader("Location", $"{this.Request.Url.SiteBase}/users/{newUser.Id}") // Adds a location header to the response because this is expected by HTTP for 201 Created responses.
                .WithModel(newUser); // Returns the user in the response for convenience.
        }

        private void AddRegisteredUser(LoyaltyProgramUser newUser)
        {
            // Store the newUser to a data store
            var userId = registeredUsers.Count;
            newUser.Id = userId;
            registeredUsers[userId] = newUser;
        }
    }
}
