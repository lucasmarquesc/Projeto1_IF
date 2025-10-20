using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Projeto1_IF.Models;
using Microsoft.AspNetCore.Authorization;

namespace Projeto1_IF.Controllers
{
    [Authorize] 
    public class TbProfissionalsController : Controller
    {
        private readonly db_IFContext _context;

        private readonly UserManager<IdentityUser> _userManager;

        // Construtor para receber o UserManager
        public TbProfissionalsController(db_IFContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager; 
        }

        // GET: TbProfissionals
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            if (User.IsInRole("Médico") || User.IsInRole("Nutricionista"))
            {
                // 1. Profissional: só vê o próprio cadastro
                var db_IFContext = _context.TbProfissionals
                    .Where(p => p.IdUser == userId) // Filtra pelo IdUser logado
                    .Include(t => t.IdCidadeNavigation)
                    .Include(t => t.IdContratoNavigation)
                    .Include(t => t.IdTipoAcessoNavigation);
                return View(await db_IFContext.ToListAsync());
            }
            else if (User.IsInRole("Gerente Médico"))
            {
                // 2. Gerente Médico: Vê todos os Médicos              
                var db_IFContext = _context.TbProfissionals
                    .Where(p => p.IdTipoProfissional == 1) // Filtra por tipo
                    .Include(t => t.IdCidadeNavigation)
                    .Include(t => t.IdContratoNavigation)
                    .Include(t => t.IdTipoAcessoNavigation);
                return View(await db_IFContext.ToListAsync());
              
            }
            else if (User.IsInRole("Gerente Nutricionista"))
            {
                // 3. Gerente Nutri: Vê todos os Nutricionistas                
                var db_IFContext = _context.TbProfissionals
                    .Where(p => p.IdTipoProfissional == 2) // Filtra por tipo
                    .Include(t => t.IdCidadeNavigation)
                    .Include(t => t.IdContratoNavigation)
                    .Include(t => t.IdTipoAcessoNavigation);
                return View(await db_IFContext.ToListAsync());
               
            }
            else if (User.IsInRole("Gerente Geral"))
            {
                // 4. Gerente Geral: Vê todos (seu código original)
                var db_IFContext = _context.TbProfissionals.Include(t => t.IdCidadeNavigation).Include(t => t.IdContratoNavigation).Include(t => t.IdTipoAcessoNavigation);
                return View(await db_IFContext.ToListAsync());
            }

            return Forbid(); // Se não for nenhum dos acima, proíbe o acesso.
        }

        // GET: TbProfissionals/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Error", "Home");
            }

            var tbProfissional = await _context.TbProfissionals
                .Include(t => t.IdCidadeNavigation)
                .Include(t => t.IdContratoNavigation)  
                .Include(t => t.IdTipoAcessoNavigation)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.IdProfissional == id);

            if (tbProfissional == null)
            {
                return RedirectToAction("Error", "Home");
            }

            // Lógica de Autorização (Whitelist)
            var userId = _userManager.GetUserId(User);

            // 1. O usuário é um profissional (Médico/Nutri) E é o dono deste perfil?
            bool isOwner = (User.IsInRole("Médico") || User.IsInRole("Nutricionista"))
                            && tbProfissional.IdUser == userId;

            // 2. O usuário é Gerente Médico E o perfil é de um Médico?
            bool isGerenteMedico = User.IsInRole("Gerente Médico")
                                   && tbProfissional.IdTipoProfissional == 1;

            // 3. O usuário é Gerente Nutri E o perfil é de um Nutricionista?            
            bool isGerenteNutri = User.IsInRole("Gerente Nutricionista")
                                  && tbProfissional.IdTipoProfissional == 2;

            // 4. O usuário é Gerente Geral? (Pode ver todos)
            bool isGerenteGeral = User.IsInRole("Gerente Geral");

            // Verifica se QUALQUER uma das permissões é verdadeira
            if (isOwner || isGerenteMedico || isGerenteNutri || isGerenteGeral)
            {
                // Se sim, permite ver os detalhes
                return View(tbProfissional);
            }

            // Se chegou até aqui, o usuário não tem nenhuma das permissões.
            return Forbid(); // Proibido (retorna um erro 403 Forbidden)

        }

        // GET: TbProfissionals/Create
        [Authorize(Roles = "Médico, Nutricionista")] // Só profissionais podem criar
        public IActionResult Create()
        {
            ViewData["IdCidade"] = new SelectList(_context.TbCidades, "IdCidade", "Nome");
            ViewData["IdPlano"] = new SelectList(_context.TbPlanos, "IdPlano", "Nome");
            ViewData["IdTipoAcesso"] = new SelectList(_context.TbTipoAcessos, "IdTipoAcesso", "Nome");
            return View();
        }

        // POST: TbProfissionals/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Médico, Nutricionista")] // Só profissionais podem criar
        public async Task<IActionResult> Create([Bind("IdProfissional,IdTipoProfissional,IdTipoAcesso,IdCidade,IdUser,Nome,Cpf,CrmCrn,Especialidade,Logradouro,Numero,Bairro,Cep,Cidade,Estado,Ddd1,Ddd2,Telefone1,Telefone2,Salario")] TbProfissional tbProfissional, [Bind("IdPlano")] TbContrato IdContratoNavigation)
        {
            try
            {
                ModelState.Remove("IdUser"); //modo de desconsiderar esse atributo e a validação ser aprovada.
                ModelState.Remove("IdContrato"); //modo de desconsiderar esse atributo e a validação ser aprovada.

                if (ModelState.IsValid)
                {
                    IdContratoNavigation.DataInicio = DateTime.UtcNow;
                    IdContratoNavigation.DataFim = IdContratoNavigation.DataInicio.Value.AddMonths(1);
                    _context.Add(IdContratoNavigation);
                    await _context.SaveChangesAsync();

                    var userManager = HttpContext.RequestServices.GetService<UserManager<IdentityUser>>();
                    if (userManager != null)
                    {
                        var email = User.Identity?.Name;
                        if (email != null)
                        {
                            var user = await userManager.FindByEmailAsync(email);
                            if (user != null)
                            {
                                tbProfissional.IdUser = user.Id;
                            }
                            else
                            {
                                return NotFound();
                            }
                        }
                        else
                        {
                            return NotFound();
                        }
                    }
                    else
                    {
                        return NotFound();
                    }

                    tbProfissional.IdContrato = IdContratoNavigation.IdContrato;
                    _context.Add(tbProfissional);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }

            }
            catch (DbUpdateException dex)
            {
                ModelState.AddModelError("", "Incapaz de salvar." + dex.ToString());
            }

            ViewData["IdCidade"] = new SelectList(_context.TbCidades, "IdCidade", "Nome");
            ViewData["IdPlano"] = new SelectList(_context.TbPlanos, "IdPlano", "Nome",IdContratoNavigation.IdPlano);
            ViewData["IdTipoAcesso"] = new SelectList(_context.TbTipoAcessos, "IdTipoAcesso", "Nome", tbProfissional.IdTipoAcesso);
            return View(tbProfissional);
        }

        // GET: TbProfissionals/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Error", "Home");
            }

            // Busca o profissional, incluindo o Contrato para o dropdown de "Plano"
            var tbProfissional = await _context.TbProfissionals
                .Include(t => t.IdContratoNavigation)
                .FirstOrDefaultAsync(s => s.IdProfissional == id);

            if (tbProfissional == null)
            {
                return NotFound();
            }

            // Lógica de Autorização (Whitelist)
            var userId = _userManager.GetUserId(User);

            // 1. O usuário é um profissional (Médico/Nutri) E é o dono deste perfil?
            bool isOwner = (User.IsInRole("Médico") || User.IsInRole("Nutricionista"))
                            && tbProfissional.IdUser == userId;

            // 2. O usuário é Gerente Médico E o perfil é de um Médico?            
            bool isGerenteMedico = User.IsInRole("Gerente Médico")
                                   && tbProfissional.IdTipoProfissional == 1;

            // 3. O usuário é Gerente Nutri E o perfil é de um Nutricionista?            
            bool isGerenteNutri = User.IsInRole("Gerente Nutricionista")
                                  && tbProfissional.IdTipoProfissional == 2;

            // 4. O usuário é Gerente Geral? (Pode ver todos)
            bool isGerenteGeral = User.IsInRole("Gerente Geral");

            // Se NÃO tiver nenhuma das permissões, proíbe o acesso.
            if (!(isOwner || isGerenteMedico || isGerenteNutri || isGerenteGeral))
            {
                return Forbid(); // Proibido (retorna um erro 403 Forbidden)
            }

            // (Seu código original para popular os dropdowns)
            ViewData["IdCidade"] = new SelectList(_context.TbCidades, "IdCidade", "Nome", tbProfissional.IdCidade);
            ViewData["IdPlano"] = new SelectList(_context.TbPlanos, "IdPlano", "Nome", tbProfissional.IdContratoNavigation.IdPlano);
            ViewData["IdTipoAcesso"] = new SelectList(_context.TbTipoAcessos, "IdTipoAcesso", "Nome", tbProfissional.IdTipoAcesso);

            return View(tbProfissional);
        }

        // POST: TbProfissionals/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tbProfissional = await _context.TbProfissionals
                .Include(t => t.IdContratoNavigation)
                .FirstOrDefaultAsync(p => p.IdProfissional == id);

            if (tbProfissional == null)
            {
                return NotFound();
            }

            // 1. Lógica de Autorização (Necessária de novo no POST)
            var userId = _userManager.GetUserId(User);
            bool isOwner = (User.IsInRole("Médico") || User.IsInRole("Nutricionista"))
                            && tbProfissional.IdUser == userId;
            bool isGerenteMedico = User.IsInRole("Gerente Médico")
                                   && tbProfissional.IdTipoProfissional == 1; 
            bool isGerenteNutri = User.IsInRole("Gerente Nutricionista")
                                  && tbProfissional.IdTipoProfissional == 2; 
            bool isGerenteGeral = User.IsInRole("Gerente Geral");

            if (!(isOwner || isGerenteMedico || isGerenteGeral || isGerenteNutri))
            {
                return Forbid(); // Proibido
            }

            // 2. Lógica de Bind Condicional (CPF)
            var allowedFields = new List<string>
    {
        // Adicionar todos os campos do Bind original, EXCETO Cpf
        "IdTipoAcesso", "IdCidade", "Nome", "CrmCrn", "Especialidade",
        "Logradouro", "Numero", "Bairro", "Cep", "Cidade", "Estado",
        "Ddd1", "Ddd2", "Telefone1", "Telefone2", "Salario"
       
    };

            // Adiciona o CPF à lista APENAS se for um Gerente
            if (isGerenteMedico || isGerenteNutri || isGerenteGeral)
            {
                allowedFields.Add("Cpf");
            }

            // 3. Tenta atualizar o modelo usando a lista de campos permitidos
            
            if (await TryUpdateModelAsync<TbProfissional>(
                tbProfissional,
                "", // Prefixo vazio
                p => allowedFields.Contains(p.PropertyName)))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException /* ex */)
                {
                    //Log the error (uncomment ex variable name and write a log.)
                    ModelState.AddModelError("", "Unable to save changes. " +
                        "Try again, and if the problem persists, " +
                        "see your system administrator.");
                }
            }

         
            ViewData["IdCidade"] = new SelectList(_context.TbCidades, "IdCidade", "Nome", tbProfissional.IdCidade);
            ViewData["IdPlano"] = new SelectList(_context.TbPlanos, "IdPlano", "Nome", tbProfissional.IdContratoNavigation.IdPlano);
            ViewData["IdTipoAcesso"] = new SelectList(_context.TbTipoAcessos, "IdTipoAcesso", "Nome", tbProfissional.IdTipoAcesso);

            return View(tbProfissional);
        }

        // GET: TbProfissionals/Delete/5
        [Authorize(Roles = "Gerente Médico, Gerente Nutricionista, Gerente Geral")]
        public async Task<IActionResult> Delete(int? id, bool? saveChangesError = false)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Busca o profissional com os dados para a View
            var tbProfissional = await _context.TbProfissionals
                .Include(t => t.IdCidadeNavigation)
                .Include(t => t.IdTipoAcessoNavigation)
                .Include(t => t.IdContratoNavigation)
                .ThenInclude(t => t.IdPlanoNavigation) // Include do Plano
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.IdProfissional == id);

            if (tbProfissional == null)
            {
                return NotFound();
            }


            // Lógica de Autorização (Whitelist) - Gerente só vê o que pode deletar

            // 2. O usuário é Gerente Médico E o perfil é de um Médico?
            
            bool isGerenteMedico = User.IsInRole("Gerente Médico")
                                   && tbProfissional.IdTipoProfissional == 1;

            // 3. O usuário é Gerente Nutri E o perfil é de um Nutricionista?
            
            bool isGerenteNutri = User.IsInRole("Gerente Nutricionista")
                                  && tbProfissional.IdTipoProfissional == 2;

            // 4. O usuário é Gerente Geral? (Pode ver todos)
            bool isGerenteGeral = User.IsInRole("Gerente Geral");

            // Se NÃO tiver nenhuma das permissões, proíbe o acesso.
            if (!(isGerenteMedico || isGerenteNutri || isGerenteGeral))
            {
                return Forbid(); // Proibido (retorna um erro 403 Forbidden)
            }
      

            // Mensagem de erro se 'DeleteConfirmed' falhar por DbUpdateException
            if (saveChangesError.GetValueOrDefault())
            {
                ViewData["ErrorMessage"] =
                    "A exclusão falhou. Tente novamente e, se o problema persistir, " +
                    "entre em contato com o administrador do sistema.";
            }

            return View(tbProfissional);
        }

        // POST: TbProfissionals/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Gerente Médico, Gerente Nutricionista, Gerente Geral")] // Só Gerentes
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Busca o profissional APENAS para verificação de escopo e pacientes
            var tbProfissional = await _context.TbProfissionals
                .AsNoTracking() // Não precisa rastrear ainda
                .FirstOrDefaultAsync(p => p.IdProfissional == id);

            if (tbProfissional == null)
            {
                // Se já foi deletado, apenas retorne
                return RedirectToAction(nameof(Index));
            }           

            // 1. Lógica de Autorização (Whitelist)
            bool isGerenteMedico = User.IsInRole("Gerente Médico")
                                   && tbProfissional.IdTipoProfissional == 1; 
            bool isGerenteNutri = User.IsInRole("Gerente Nutricionista")
                                  && tbProfissional.IdTipoProfissional == 2;
            bool isGerenteGeral = User.IsInRole("Gerente Geral");

            if (!(isGerenteMedico || isGerenteNutri || isGerenteGeral))
            {
                return Forbid(); // Proibido
            }


            // 2. Verificação de Pacientes
            
            bool hasPatients = await _context.TbMedicoPacientes
                                             .AnyAsync(mp => mp.IdProfissional == id);

            if (hasPatients)
            {
                // Tem pacientes, não pode excluir.
                ViewData["ErrorMessage"] = "Exclusão falhou. Este profissional possui pacientes cadastrados.";

                // Recarrega os dados completos para a view de Delete (como no método GET)
                var profissionalComErro = await _context.TbProfissionals
                    .Include(t => t.IdCidadeNavigation)
                    .Include(t => t.IdTipoAcessoNavigation)
                    .Include(t => t.IdContratoNavigation)
                    .ThenInclude(t => t.IdPlanoNavigation)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(m => m.IdProfissional == id);

                return View(nameof(Delete), profissionalComErro);
            }

            // 3. Se não tiver pacientes e for autorizado, pode excluir
            try
            {
                // Buscar a entidade novamente para Rastrear e Remover
                
                var profissionalParaDeletar = new TbProfissional { IdProfissional = id };
                _context.TbProfissionals.Attach(profissionalParaDeletar);
                _context.TbProfissionals.Remove(profissionalParaDeletar);
               
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException /* ex */)
            {
                //Log the error (uncomment ex variable name and write a log.)
                return Forbid(); // Proibido
            }
        }

    }
}
