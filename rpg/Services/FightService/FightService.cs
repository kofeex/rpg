using AutoMapper;
using Microsoft.EntityFrameworkCore;
using rpg.Data;
using rpg.Dtos.Fight;
using rpg.Models;

namespace rpg.Services.FightService
{
    public class FightService : IFightService
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public FightService(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }


        /* Method where selected characters fight automatically.
         Winner is the character that deliveres the finishing blow to an opponent who becomes the loser */
        public async Task<ServiceResponse<FightResultDto>> Fight(FightRequestDto request)
        {
            var serviceResponse = new ServiceResponse<FightResultDto>
            {
                Data = new FightResultDto()
            };

            try
            {
                var characters = await _context.Characters
                    .Include(c => c.Weapon)
                    .Include(c => c.Skills)
                    .Where(c => request.CaracterIds.Contains(c.Id))
                    .ToListAsync();

                bool defeated = false;

                while(!defeated)
                {
                    foreach(var attacker in characters)
                    {
                        var opponents = characters.Where(c => c.Id != attacker.Id).ToList();
                        var opponent = opponents[new Random().Next(opponents.Count)];

                        int damage = 0;
                        string attackUsed = string.Empty;

                        bool useWeapon = new Random().Next(2) == 0;
                        if (useWeapon && attacker.Weapon != null)
                        {
                            attackUsed = attacker.Weapon.Name;
                            damage = DoWeaponAttack(attacker, opponent);
                        }
                        else if (!useWeapon && attacker.Skills != null)
                        {
                            var skill = attacker.Skills[new Random().Next(attacker.Skills.Count)];
                            attackUsed = skill.Name;
                            damage = DoSkillAttack(attacker, opponent, skill);
                        }
                        else
                        {
                            serviceResponse.Data.Log.Add($"{attacker.Name} wasn't able to attack!");
                            continue;
                        }

                        serviceResponse.Data.Log
                            .Add($"{attacker.Name} attacks {opponent.Name} using {attackUsed} with {(damage >= 0 ? damage : 0)} damage");

                        if (opponent.HitPoints <= 0)
                        {
                            defeated = true;
                            attacker.Victories++;
                            opponent.Defeats++;
                            serviceResponse.Data.Log.Add($"{opponent.Name} has been defeated");
                            serviceResponse.Data.Log.Add($"{attacker.Name} wins with {attacker.HitPoints} HP left!");
                            break;
                        }
                    }
                }

                characters.ForEach(c =>
                {
                    c.Fights++;
                    c.HitPoints = 100;
                });

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }

            return serviceResponse;
        }

        public async Task<ServiceResponse<AttackResultDto>> SkillAttack(SkillAttackDto request)
        {
            var serviceResponse = new ServiceResponse<AttackResultDto>();

            try
            {
                var attacker = await _context.Characters
                    .Include(c => c.Skills)
                    .FirstOrDefaultAsync(c => c.Id == request.AttackerId);

                var opponent = await _context.Characters
                    .FirstOrDefaultAsync(c => c.Id == request.OpponentId);

                if (attacker == null || opponent == null || attacker.Skills == null)
                {
                    throw new Exception("Something fishy is going on");
                }

                var skill = attacker.Skills.FirstOrDefault(s => s.Id == request.SkillId);

                if (skill == null)
                {
                    serviceResponse.Success = false;
                    serviceResponse.Message = $"{attacker.Name} doesn't know that skill!";
                    return serviceResponse;
                }

                int damage = DoSkillAttack(attacker, opponent, skill);

                if (opponent.HitPoints <= 0)
                {
                    serviceResponse.Message = $"{opponent.Name} has been defeated!";
                }

                await _context.SaveChangesAsync();

                serviceResponse.Data = new AttackResultDto
                {
                    Attacker = attacker.Name,
                    Opponent = opponent.Name,
                    AttackerHP = attacker.HitPoints,
                    OpponentHP = opponent.HitPoints,
                    Damage = damage
                };
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }

            return serviceResponse;
        }

        private static int DoSkillAttack(Character attacker, Character opponent, Skill skill)
        {
            int damage = skill.Damage + (new Random().Next(attacker.Intelligence));
            damage -= new Random().Next(opponent.Defence);

            if (damage > 0)
            {
                opponent.HitPoints -= damage;
            }

            return damage;
        }

        public async Task<ServiceResponse<AttackResultDto>> WeaponAttack(WeaponAttackDto request)
        {
            var serviceResponse = new ServiceResponse<AttackResultDto>();

            try
            {
                var attacker = await _context.Characters
                    .Include(c => c.Weapon)
                    .FirstOrDefaultAsync(c => c.Id == request.AttackerId);

                var opponent = await _context.Characters
                    .FirstOrDefaultAsync(c => c.Id == request.OpponentId);

                if (attacker == null || opponent == null || attacker.Weapon == null)
                {
                    throw new Exception("Something fishy is going on");
                }

                int damage = DoWeaponAttack(attacker, opponent);

                if (opponent.HitPoints <= 0)
                {
                    serviceResponse.Message = $"{opponent.Name} has been defeated!";
                }

                await _context.SaveChangesAsync();

                serviceResponse.Data = new AttackResultDto
                {
                    Attacker = attacker.Name,
                    Opponent = opponent.Name,
                    AttackerHP = attacker.HitPoints,
                    OpponentHP = opponent.HitPoints,
                    Damage = damage
                };
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }

            return serviceResponse;
        }

        private static int DoWeaponAttack(Character attacker, Character opponent)
        {
            if (attacker.Weapon == null)
            {
                throw new Exception("Attacker has no weapon!");
            }

            int damage = attacker.Weapon.Damage + (new Random().Next(attacker.Strength));
            damage -= new Random().Next(opponent.Defence);

            if (damage > 0)
            {
                opponent.HitPoints -= damage;
            }

            return damage;
        }

        public async Task<ServiceResponse<List<HighscoreDto>>> GetHighscore()
        {
            var characters = await _context.Characters
                .Where(c => c.Fights > 0)
                .OrderByDescending(c => c.Victories)
                .ThenBy(c => c.Defeats)
                .ToListAsync();

            var response = new ServiceResponse<List<HighscoreDto>>()
            {
                Data = characters.Select(c => _mapper.Map<HighscoreDto>(c)).ToList()
            };

            return response;
        }
    }
}
