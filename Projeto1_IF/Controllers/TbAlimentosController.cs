﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Projeto1_IF.Models;
//Lucas Marques da Cunha
namespace Projeto1_IF.Controllers
{
    [Authorize]
    public class TbAlimentosController : Controller
    {
        private readonly db_IFContext _context;

        public TbAlimentosController(db_IFContext context)
        {
            _context = context;
        }

        // GET: TbAlimentos
        public async Task<IActionResult> Index()
        {
            return View(await _context.TbAlimentos.ToListAsync());
        }

        // GET: TbAlimentos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tbAlimento = await _context.TbAlimentos
                .FirstOrDefaultAsync(m => m.IdAlimento == id);
            if (tbAlimento == null)
            {
                return NotFound();
            }

            return View(tbAlimento);
        }

        // GET: TbAlimentos/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: TbAlimentos/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdAlimento,IdTipoQuantidade,Nome,Carboidrato,VitaminaA,VitaminaB")] TbAlimento tbAlimento)
        {
            if (ModelState.IsValid)
            {
                _context.Add(tbAlimento);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(tbAlimento);
        }

        // GET: TbAlimentos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tbAlimento = await _context.TbAlimentos.FindAsync(id);
            if (tbAlimento == null)
            {
                return NotFound();
            }
            return View(tbAlimento);
        }

        // POST: TbAlimentos/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdAlimento,IdTipoQuantidade,Nome,Carboidrato,VitaminaA,VitaminaB")] TbAlimento tbAlimento)
        {
            if (id != tbAlimento.IdAlimento)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tbAlimento);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TbAlimentoExists(tbAlimento.IdAlimento))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(tbAlimento);
        }

        // GET: TbAlimentos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tbAlimento = await _context.TbAlimentos
                .FirstOrDefaultAsync(m => m.IdAlimento == id);
            if (tbAlimento == null)
            {
                return NotFound();
            }

            return View(tbAlimento);
        }

        // POST: TbAlimentos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tbAlimento = await _context.TbAlimentos.FindAsync(id);
            if (tbAlimento != null)
            {
                _context.TbAlimentos.Remove(tbAlimento);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TbAlimentoExists(int id)
        {
            return _context.TbAlimentos.Any(e => e.IdAlimento == id);
        }
    }
}
