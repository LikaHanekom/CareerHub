using CareerHub.Api.Models;

namespace CareerHub.Api.Repositories
{
    public interface ICompanyRepository
    {
        Task<IEnumerable<Company>> GetAllCompaniesAsync();
        Task<Company?> GetCompanyByIdAsync(Guid id);
        Task<bool> DoesCompanyExistByNameAsync(string name);
        Task<Company> CreateCompanyAsync(Company company);
    }
}