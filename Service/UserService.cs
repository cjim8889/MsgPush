using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MsgPush.Context;
using MsgPush.Model;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;

namespace MsgPush.Service
{
    public class UserService
    {
        private readonly IMongoCollection<User> users;
        private readonly IConfiguration configuration;
        public UserService(DatabaseContext databaseContext, IConfiguration configuration)
        {
            users = databaseContext.Database.GetCollection<User>("Users");
            this.configuration = configuration;
        }

        public class TokenPair
        {
            public string AdminToken { get; set; }
            public string PushToken { get; set; }
        }

        public async Task CreateUserAsync(User user)
        {
            user.Password = HashString(user.Password);

            await users.InsertOneAsync(user);
        }

        public async Task<List<User>> GetUsersAsync()
        {
            return await users.Find(x => true).ToListAsync();
        }

        public static string GenerateToken()
        {
            string token;

            using (RandomNumberGenerator rng = new RNGCryptoServiceProvider())
            {
                byte[] TokenData = new byte[32];

                rng.GetBytes(TokenData);

                token = WebEncoders.Base64UrlEncode(TokenData);
            }

            return token;
        }

        public string GenerateJwtToken(User user, DateTime expiryDate)
        {
            var claims = GenerateClaims(user);
                       
            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(configuration.GetSection("Jwt_SecretKey").Value));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: configuration.GetValue<string>("Jwt:Issuer"),
                audience: configuration.GetValue<string>("Jwt:Audience"),
                expires: expiryDate,
                claims: claims,
                signingCredentials: creds
                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string HashString(string str)
        {
            byte[] salt = Encoding.ASCII.GetBytes(configuration.GetSection("SecretKey").Value);

            return Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: str,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 100,
                numBytesRequested: 256 / 8
                ));
        }

        private static List<Claim> GenerateClaims(User user)
        {
            var claims = new List<Claim>();

            AddRolesToClaims(claims, user.Roles);

            claims.Add(new Claim(ClaimTypes.Name, user.Username));
            claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id));

            return claims;
        }

        public async Task RemoveUserByIdAsync(string id)
        {
            await users.DeleteOneAsync(x => x.Id == id);
        }

        public async Task<TokenPair> RefreshUserToken(string id)
        {

            var adminToken = GenerateToken();
            var pushToken = GenerateToken();

            var update = Builders<User>.Update.Set("AdminToken", adminToken).Set("PushToken", pushToken);
            var result = await users.UpdateOneAsync(x => x.Id == id, update);

            if (result.IsAcknowledged)
            {
                return new TokenPair() { AdminToken = adminToken, PushToken = pushToken };
            }

            return null;
        }

        public async Task<bool> AddSubsriberToUserAsync(string id, long subsriberId)
        {
            var update = Builders<User>.Update.AddToSet("Subsribers", subsriberId);
            var result = await users.UpdateOneAsync(x => x.Id == id, update);

            return result.IsAcknowledged;
        }

        public async Task<bool> AddRoleToUserAsync(string id, string role)
        {
            return await AddRolesToUserAsync(id, new string[] { role });
        }

        public async Task<bool> AddRolesToUserAsync(string id, string[] roles)
        {   
            var update = Builders<User>.Update.AddToSetEach("Roles", roles);


            var updateResult = await users.UpdateOneAsync(x => x.Id == id, update);

            return updateResult.IsAcknowledged;
        }

        public async Task<bool> RemoveRoleOfUserAsync(string id, string role)
        {
            return await RemoveRolesOfUserAsync(id, new string[] { role });
        }

        public async Task<bool> RemoveRolesOfUserAsync(string id, string[] role)
        {
            var update = Builders<User>.Update.PullAll("Roles", role);
            var updateResult = await users.UpdateOneAsync(x => x.Id == id, update);

            return updateResult.IsAcknowledged;
        }

        public async Task<bool> SetUserValidationAsync(string id, bool state)
        {
            var update = Builders<User>.Update.Set("Validated", state);
            var updateResult = await users.UpdateOneAsync(x => x.Id == id, update);

            return updateResult.IsAcknowledged;
        }

        public async Task<User> GetUserByIdAsync(string id)
        {
            return await users.Find(x => x.Id == id).FirstOrDefaultAsync();
        }

        public async Task<User> GetUserByAdminTokenAsync(string adminToken)
        {
            return await users.Find(x => x.AdminToken == adminToken).FirstOrDefaultAsync();
        }

        public async Task<User> GetUserByUsernameAndPasswordAsync(string username, string password)
        {
            return await users.Find(x => x.Username == username & x.Password == HashString(password)).FirstOrDefaultAsync();
        }

        public async Task<bool> IsUsernameExistsAsync(string Username)
        {
            var user = await users.Find(x => x.Username == Username).FirstOrDefaultAsync();

            return user != null;
        }

        public async Task<bool> ChangeUserPassword(string id, string password)
        {
            if (!ValidatePassword(password))
            {
                return false;
            }

            var update = Builders<User>.Update.Set("Password", HashString(password));
            var updateResult = await users.UpdateOneAsync(x => x.Id == id, update);

            return updateResult.IsAcknowledged;
        }


        public bool ValidateUserData(User user)
        {
            return ValidatePassword(user.Password) & ValidateUsername(user.Username);
        }

        private bool ValidateUsername(string Username)
        {
            if (string.IsNullOrWhiteSpace(Username))
            {
                return false;
            }

            return true;
        }
        private bool ValidatePassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
            {
                return false;
            }
            else if (password.Length > 64)
            {
                return false;
            }

            return true;
        }

        private static void AddRolesToClaims(List<Claim> claims, IEnumerable<string> roles)
        {
            foreach (string role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
        }
    }
}
