using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using rpg.Dtos.Character;
using rpg.Dtos.Weapon;
using rpg.Models;
using rpg.Services.WeaponService;

namespace rpg.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class WeaponController : ControllerBase
    {
        private readonly IWeaponService _weaponService;
        public WeaponController(IWeaponService weaponService)
        {
            _weaponService = weaponService;
        }

        [HttpPost]
        public async Task<ActionResult<ServiceResponse<GetCharacterDto>>> AddWeapon(AddWeaponDto newWeapon)
        {
            var response = await _weaponService.AddWeapon(newWeapon);

            if (response.Data == null)
            {
                return NotFound(response);
            }

            return Ok(response);
        }
    }
}
