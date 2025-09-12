using Api.Domain.Entities;
using Api.Domain.ValueObjects;

namespace Api.Application.Interfaces;

public interface IUserRepository
{
    User GetById(UserId userId);
    User GetByName(string name);
    IEnumerable<User> GetAll();
    void Save(User user);
}
