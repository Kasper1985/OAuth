using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

using Models;
using BusinessLogic.Interfaces;
using Data.Interfaces;

namespace BusinessLogic
{
    public class UserLogic : IUserLogic
    {
        private readonly IUserSource userSource;
        private readonly IMailLogic mailLogic;

        public UserLogic(IUserSource userSource, IMailLogic mailLogic)
        {
            this.userSource = userSource ?? throw new ArgumentNullException(nameof(userSource));
            this.mailLogic = mailLogic ?? throw new ArgumentNullException(nameof(mailLogic));
        }

        #region Interface functions
        public async Task<User> GetUserAsync(int userId) => await userSource.GetUserAsync(userId);

        public async Task<User> LoginAsync(string login, string password) => await userSource.LoginAsync(login, password);

        public async Task GenerateNewPasswordAsync(string email, string nameLast = null, string nameFirst = null)
        {
            var user = await userSource.GetUserAsync(email);
            if (string.Compare(user.NameLast, nameLast, ignoreCase: true) != 0 || string.Compare(user.NameFirst, nameFirst, ignoreCase: true) != 0)
                throw new OperationCanceledException("Wrong name or email");

            var password = GenerateNewPasswordString();
            await Task.WhenAll(this.userSource.SaveNewPasswordAsync(password, user.Id), mailLogic.SendPasswordMailAsync(password, user));
        }

        private static string GenerateNewPasswordString()
        {
            // Letters in groups are listed several times to nigilize Gaussian distribution of the probabilities
            var allowedChars = new Dictionary<int, string>
            {
                { 0, "abcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyz" },
                { 1, "ABCDEFGHIJKLMNOPQRSTUVWXYZABCDEFGHIJKLMNOPQRSTUVWXYZABCDEFGHIJKLMNOPQRSTUVWXYZ" },
                { 2, "012345678901234567890123456789" },
                { 3, "'-!\"#$%&()*,./:;?@[\\]^_`{|}´+=§'-!\"#$%&()*,./:;?@[\\]^_`{|}´+=§'-!\"#$%&()*,./:;?@[\\]^_`{|}´+=§" }
            };

            // To make pseudo random generator more rando, a seed will be
            // created and headed over to the generator. The seed will be
            // created based on four random bytes provided by a Windows
            // Crypto Service Provider.
            var randomBytes = new byte[4];
            var rng = new RNGCryptoServiceProvider();
            rng.GetBytes(randomBytes);
            var seed = ((randomBytes[0] & 0x7f) << 24) | (randomBytes[1] << 16) | (randomBytes[2] << 8) | randomBytes[3];
            var random = new Random(seed);

            // Define a length between 8 and 16 chars of a new password
            var lenght = random.Next(8, 16);
            var password = "";
            var groupsUsed = new List<int>();

            // generating password of a given length by using each char group
            while (password.Length < lenght)
            {
                // Randomly choose a character group and a character of a password
                var group = random.Next(0, allowedChars.Count);
                password += allowedChars[group][random.Next(0, allowedChars[group].Length - 1)];

                // Remember the used group for later check
                if (!groupsUsed.Contains(group))
                    groupsUsed.Add(group);

                // Check if all groups were used and force a group if there is no more enough tries left.
                // This should have real rare occuring rate!!!
                if (password.Length - lenght > allowedChars.Count - groupsUsed.Count) continue;
                foreach (var key in allowedChars.Keys.Where(key => !groupsUsed.Contains(key)))
                {
                    password += allowedChars[key][random.Next(0, allowedChars[key].Length - 1)];
                    groupsUsed.Add(key);
                }
            }

            return password;
        }
        #endregion // Interface functions
    }
}