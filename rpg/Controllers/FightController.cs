using Microsoft.AspNetCore.Mvc;
using rpg.Dtos.Fight;
using rpg.Models;
using rpg.Services.FightService;

namespace rpg.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FightController : ControllerBase
    {
        private readonly IFightService _fightService;

        public FightController(IFightService fightService)
        {
            _fightService = fightService;
        }

        [HttpPost("Weapon")]
        public async Task<ActionResult<ServiceResponse<AttackResultDto>>> WeaponAttack(WeaponAttackDto request)
        {
            var response = await _fightService.WeaponAttack(request);

            if (response.Data == null)
            {
                return NotFound(response);
            }

            return Ok(response);
        }

        [HttpPost]
        public async Task<ActionResult<ServiceResponse<AttackResultDto>>> SkillAttack(SkillAttackDto request)
        {
            var response = await _fightService.SkillAttack(request);

            if (response.Data == null)
            {
                return NotFound(response);
            }

            return Ok(response);
        }

        [HttpPost("Fight")]
        public async Task<ActionResult<ServiceResponse<FightResultDto>>> Fight(FightRequestDto request)
        {
            var response = await _fightService.Fight(request);

            if (response.Data == null)
            {
                return NotFound(response);
            }

            return Ok(response);
        }

        [HttpGet]
        public async Task<ActionResult<ServiceResponse<List<HighscoreDto>>>> GetHighscore()
        {
            var response = await _fightService.GetHighscore();

            if (response.Data == null)
            {
                return NotFound(response);
            }

            return Ok(response);
        }
    }
}
