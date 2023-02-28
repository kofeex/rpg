using AutoMapper;
using rpg.Dtos.Character;
using rpg.Dtos.Skill;
using rpg.Dtos.Weapon;
using rpg.Models;

namespace rpg
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Character, GetCharacterDto>();
            CreateMap<AddCharacterDto, Character>();
            CreateMap<Weapon, GetWeaponDto>();
            CreateMap<Skill, GetSkillDto>();
        }
    }
}
