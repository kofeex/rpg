using AutoMapper;
using Microsoft.EntityFrameworkCore;
using rpg.Data;
using rpg.Dtos.Character;
using rpg.Dtos.Weapon;
using rpg.Models;
using System.Security.Claims;

namespace rpg.Services.WeaponService
{
    public class WeaponService : IWeaponService
    {
        private readonly DataContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMapper _mapper;

        public WeaponService(DataContext context, IHttpContextAccessor httpContextAccessor, IMapper mapper)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _mapper = mapper;
        }

        private int GetUserId() => int.Parse(_httpContextAccessor.HttpContext!.User
            .FindFirstValue(ClaimTypes.NameIdentifier)!);

        public async Task<ServiceResponse<GetCharacterDto>> AddWeapon(AddWeaponDto newWeapon)
        {
            var serviceResponse = new ServiceResponse<GetCharacterDto>();

            try
            {
                var character = await _context.Characters
                    .FirstOrDefaultAsync(c => c.Id == newWeapon.CharacterId && c.User!.Id == GetUserId());

                if (character == null)
                {
                    serviceResponse.Success = false;
                    serviceResponse.Message = "Character not found!";
                    return serviceResponse;
                }

                var weapon = new Weapon
                {
                    Name = newWeapon.Name,
                    Damage = newWeapon.Damage,
                    Character = character
                };

                _context.Weapons.Add(weapon);
                await _context.SaveChangesAsync();

                serviceResponse.Data = _mapper.Map<GetCharacterDto>(character);
            } 
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }

            return serviceResponse;
        }
    }
}
