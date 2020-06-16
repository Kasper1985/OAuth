using System;
using System.Collections.Generic;
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
        public async Task<User> GetUserAsync(int userId)
        {
            return await this.userSource.GetUserAsync(userId);
        }

        public async Task<User> LoginAsync(string login, string password)
        {
            return await this.userSource.LoginAsync(login, password);
        }

        public async Task GenerateNewPasswodAsync(string email, string nameLast = null, string nameFirst = null)
        {
            User user = await this.userSource.GetUserAsync(email);
            if (string.Compare(user.NameLast, nameLast, ignoreCase: true) != 0 || string.Compare(user.NameFirst, nameFirst, ignoreCase: true) != 0)
                throw new OperationCanceledException("Wrong name or email");

            string password = this.GenerateNewPasswordString();
            await Task.WhenAll(this.userSource.SaveNewPasswordAsync(password, user.ID), this.mailLogic.SendPasswordMailAsync(password, user));
        }

        private string GenerateNewPasswordString()
        {
            // Letters in groups are listed several times to nigilize Gaussian distribution of the probabilities
            Dictionary<int, string> allowedChars = new Dictionary<int, string>
            {
                { 0, "abcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyz" },
                { 1, "ABCDEFGHIJKLMNOPQRSTUVWXYZABCDEFGHIJKLMNOPQRSTUVWXYZABCDEFGHIJKLMNOPQRSTUVWXYZ" },
                { 2, "012345678901234567890123456789" },
                { 3, "'-!\"#$%&()*,./:;?@[\\]^_`{|}´+=§'-!\"#$%&()*,./:;?@[\\]^_`{|}´+=§'-!\"#$%&()*,./:;?@[\\]^_`{|}´+=§" }
            };

            // To make pseydo random generator more rando, a seed will be
            // created and headed over to the generator. The seed will be
            // created based on four random byted provided by a Windows
            // Crypro Servcer Provider.
            var randomBytes = new byte[4];
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            rng.GetBytes(randomBytes);
            int seed = ((randomBytes[0] & 0x7f) << 24) | (randomBytes[1] << 16) | (randomBytes[2] << 8) | randomBytes[3];
            Random random = new Random(seed);

            // Define a length between 8 and 16 chars of a new password
            int lenght = random.Next(8, 16);
            string password = "";
            List<int> groupsUsed = new List<int>();

            // generating password of a given length by using each char group
            while (password.Length < lenght)
            {
                // Randomly choose a character group and a charcter of a password
                int group = random.Next(0, allowedChars.Count);
                password += allowedChars[group][random.Next(0, allowedChars[group].Length - 1)];

                // Remember the used group for later check
                if (!groupsUsed.Contains(group))
                    groupsUsed.Add(group);

                // Check if all groups were used and force a group if there is no more enough tries left.
                // This should heve real rare occuring rate!!!
                if (password.Length - lenght <= allowedChars.Count - groupsUsed.Count)
                    foreach (int key in allowedChars.Keys)
                        if (!groupsUsed.Contains(key))
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
