using AutoMapper;

namespace Simplebank.Application.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Domain.Database.Models.User, Domain.Models.Users.UserDto>();
        CreateMap<Domain.Models.Users.UserDto, Domain.Database.Models.User>();
    }
}