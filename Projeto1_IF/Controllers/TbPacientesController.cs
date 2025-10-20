using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Projeto1_IF.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Projeto1_IF.Controllers
{
    // Aautorização para Médicos e Nutricionistas em TODO o controller
    [Authorize(Roles = "Médico, Nutricionista")]
    public class TbPacientesController : Controller
    {
        private readonly db_IFContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public TbPacientesController(db_IFContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: TbPacientes
        public async Task<IActionResult> Index()
        {
            
            // 1. Obter o IdProfissional do usuário logado
            var userId = _userManager.GetUserId(User);
            var profissional = await _context.TbProfissionals
                                             .AsNoTracking()
                                             .FirstOrDefaultAsync(p => p.IdUser == userId);

            if (profissional == null) return Forbid(); // Não é um profissional

            // 2. Filtrar pacientes que pertencem a este profissional
            var db_IFContext = _context.TbPacientes
                .Include(t => t.IdCidadeNavigation)
                .Where(p => p.TbMedicoPacientes.Any(mp => mp.IdProfissional == profissional.IdProfissional));

            return View(await db_IFContext.ToListAsync());
            
        }

        // GET: TbPacientes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            
            var userId = _userManager.GetUserId(User);
            var profissional = await _context.TbProfissionals
                                             .AsNoTracking()
                                             .FirstOrDefaultAsync(p => p.IdUser == userId);
            if (profissional == null) return Forbid();

            // Busca o paciente E verifica se ele pertence ao profissional
            var tbPaciente = await _context.TbPacientes
                .Include(t => t.IdCidadeNavigation)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.IdPaciente == id &&
                                          m.TbMedicoPacientes.Any(mp => mp.IdProfissional == profissional.IdProfissional));

            if (tbPaciente == null)
            {
                // Não achou OU não pertence ao profissional
                return NotFound();
            }
            

            return View(tbPaciente);
        }

        // GET: TbPacientes/Create
        public IActionResult Create()
        {
            ViewData["IdCidade"] = new SelectList(_context.TbCidades, "IdCidade", "Nome");
            return View();
        }

        // POST: TbPacientes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdPaciente,Nome,Rg,Cpf,DataNascimento,NomeResponsavel,Sexo,Etnia,Endereco,Bairro,IdCidade,TelResidencial,TelComercial,TelCelular,Profissao,FlgAtleta,FlgGestante")] TbPaciente tbPaciente)
        {
            try
            {
                ModelState.Remove("IdPaciente");
                if (ModelState.IsValid)
                {

                    // 1. Obter o IdProfissional do usuário logado
                    var userId = _userManager.GetUserId(User); 
                    var profissional = await _context.TbProfissionals
                                                     .AsNoTracking()
                                                     .FirstOrDefaultAsync(p => p.IdUser == userId);

                    if (profissional == null)
                    {
                        return Forbid();
                    }

                    // 2. Salvar o paciente PRIMEIRO
                    _context.Add(tbPaciente);
                    await _context.SaveChangesAsync();

                    // 3. Criar o vínculo na tabela TbMedicoPaciente 
                    TbMedicoPaciente vinculo = new TbMedicoPaciente
                    {
                        IdProfissional = profissional.IdProfissional,
                        IdPaciente = tbPaciente.IdPaciente,
                        InformacaoResumida = "Paciente cadastrado"
                    };
                    _context.Add(vinculo);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
            }
            catch (DbUpdateException dex)
            {
                ModelState.AddModelError("", "Incapaz de salvar." + dex.ToString());
            }
            ViewData["IdCidade"] = new SelectList(_context.TbCidades, "IdCidade", "Nome", tbPaciente.IdCidade);
            return View(tbPaciente);
        }

        // GET: TbPacientes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return RedirectToAction("Error", "Home");
        
            var userId = _userManager.GetUserId(User);
            var profissional = await _context.TbProfissionals
                                             .AsNoTracking()
                                             .FirstOrDefaultAsync(p => p.IdUser == userId);
            if (profissional == null) return Forbid();

            // Busca o paciente E verifica se ele pertence ao profissional
            var tbPaciente = await _context.TbPacientes
                .FirstOrDefaultAsync(m => m.IdPaciente == id &&
                                          m.TbMedicoPacientes.Any(mp => mp.IdProfissional == profissional.IdProfissional));

            if (tbPaciente == null)
            {
                return NotFound();
            }
            

            ViewData["IdCidade"] = new SelectList(_context.TbCidades, "IdCidade", "Nome", tbPaciente.IdCidade);
            return View(tbPaciente);
        }

        // POST: TbPacientes/Edit/5
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(int? id)
        {
            if (id == null) return RedirectToAction("Error", "Home");

            
            var userId = _userManager.GetUserId(User);
            var profissional = await _context.TbProfissionals
                                             .AsNoTracking()
                                             .FirstOrDefaultAsync(p => p.IdUser == userId);
            if (profissional == null) return Forbid();

            // Busca o paciente E verifica se ele pertence ao profissional
            var tbPaciente = await _context.TbPacientes
                .FirstOrDefaultAsync(m => m.IdPaciente == id &&
                                          m.TbMedicoPacientes.Any(mp => mp.IdProfissional == profissional.IdProfissional));

            if (tbPaciente == null)
            {
                return NotFound();
            }
            

            if (await TryUpdateModelAsync<TbPaciente>(
                tbPaciente,
                "",
                p => p.Nome, p => p.Rg, p => p.Cpf, p => p.DataNascimento, p => p.NomeResponsavel,
                p => p.Sexo, p => p.Etnia, p => p.Endereco, p => p.Bairro, p => p.IdCidade,
                p => p.TelResidencial, p => p.TelComercial, p => p.TelCelular, p => p.Profissao,
                p => p.FlgAtleta, p => p.FlgGestante
            ))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException /* ex */)
                {
                    ModelState.AddModelError("", "Unable to save changes. ...");
                }
            }

            ViewData["IdCidade"] = new SelectList(_context.TbCidades, "IdCidade", "Nome", tbPaciente.IdCidade);
            return View(tbPaciente);
        }

        // GET: TbPacientes/Delete/5
        public async Task<IActionResult> Delete(int? id, bool? saveChangesError = false)
        {
            if (id == null) return NotFound();

            
            var userId = _userManager.GetUserId(User);
            var profissional = await _context.TbProfissionals
                                             .AsNoTracking()
                                             .FirstOrDefaultAsync(p => p.IdUser == userId);
            if (profissional == null) return Forbid();

            // Busca o paciente E verifica se ele pertence ao profissional
            var tbPaciente = await _context.TbPacientes
                .Include(t => t.IdCidadeNavigation)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.IdPaciente == id &&
                                          m.TbMedicoPacientes.Any(mp => mp.IdProfissional == profissional.IdProfissional));

            if (tbPaciente == null)
            {
                return NotFound();
            }
            

            if (saveChangesError.GetValueOrDefault())
            {
                ViewData["ErrorMessage"] = "A exclusão falhou. ...";
            }

            return View(tbPaciente);
        }

        // POST: TbPacientes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            
            var userId = _userManager.GetUserId(User);
            var profissional = await _context.TbProfissionals
                                             .AsNoTracking()
                                             .FirstOrDefaultAsync(p => p.IdUser == userId);
            if (profissional == null) return Forbid();

            // Busca o paciente E verifica se ele pertence ao profissional
            var tbPaciente = await _context.TbPacientes
                .FirstOrDefaultAsync(m => m.IdPaciente == id &&
                                          m.TbMedicoPacientes.Any(mp => mp.IdProfissional == profissional.IdProfissional));

            if (tbPaciente == null)
            {
                // Já foi deletado ou não pertence ao usuário
                return RedirectToAction(nameof(Index));
            }
            

            try
            {
                

                // Vamos remover o vínculo manualmente para garantir
                var vinculos = _context.TbMedicoPacientes
                               .Where(mp => mp.IdPaciente == id && mp.IdProfissional == profissional.IdProfissional);
                _context.TbMedicoPacientes.RemoveRange(vinculos);

                // Agora remove o paciente
                _context.TbPacientes.Remove(tbPaciente);

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException /* ex */)
            {
                return RedirectToAction(nameof(Delete), new { id = id, saveChangesError = true });
            }
        }
    }
}