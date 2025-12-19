using CarRental.Core.Entities;
using CarRental.Core.Interfaces;
using CarRental.Core.Enums;
using System.Security.Cryptography;
using System.Text;

namespace CarRental.Services
{
    public interface IAuthService
    {
        Task<User?> LoginBackOfficeAsync(string email, string password);
        Task<Client?> LoginFrontOfficeAsync(string email, string password);
        Task<Client> RegisterClientAsync(Client client, string password);
    }

    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AuthService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<User?> LoginBackOfficeAsync(string email, string password)
        {
            var users = await _unitOfWork.Repository<User>().FindAsync(u => u.Email == email);
            var user = users.FirstOrDefault();
            
            // TODO: Real password hashing check
            if (user != null && user.PasswordHash == password) 
            {
                return user;
            }
            return null;
        }

        public async Task<Client?> LoginFrontOfficeAsync(string email, string password)
        {
            var clients = await _unitOfWork.Repository<Client>().FindAsync(c => c.Email == email);
            var client = clients.FirstOrDefault();
             // TODO: Real password hashing check
            if (client != null && client.PasswordHash == password)
            {
                return client;
            }
            return null;
        }

        public async Task<Client> RegisterClientAsync(Client client, string password)
        {
            var existing = await _unitOfWork.Repository<Client>().FindAsync(c => c.Email == client.Email);
            if (existing.Any()) throw new Exception("Email already exists");

            client.PasswordHash = password; // TODO: Hash
            client.CreatedAt = DateTime.UtcNow;
            
            await _unitOfWork.Repository<Client>().AddAsync(client);
            await _unitOfWork.CompleteAsync();
            
            return client;
        }
    }
}
