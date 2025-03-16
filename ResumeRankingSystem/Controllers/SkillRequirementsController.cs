using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Domain.DataAccess;
using Domain.Entities;

namespace ResumeRankingSystem.Controllers
{
    public class SkillRequirementsController : Controller
    {
        private readonly DatabaseDbContext _context;

        public SkillRequirementsController(DatabaseDbContext context)
        {
            _context = context;
        }

        // GET: SkillRequirements
        public async Task<IActionResult> Index()
        {
            return View(await _context.SkillRequirements.ToListAsync());
        }

        // Get: All Skill Requirements for a specific Job
        [HttpGet]
        public async Task<IActionResult> GetByJobId(int jobId)
        {
            // Retrieve all skill requirements associated with the specified JobId
            var skillRequirements = await _context.SkillRequirements
                .Where(sr => sr.JobId == jobId)
                .ToListAsync();

            // Check if any skill requirements were found
            if (skillRequirements == null || !skillRequirements.Any())
            {
                return NotFound();
            }

            // Pass the jobId to the view
            ViewData["JobId"] = jobId;

            return View("SkillList", skillRequirements);
        }

        // GET: SkillRequirements/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var skillRequirement = await _context.SkillRequirements
                .FirstOrDefaultAsync(m => m.SkillRequirementId == id);
            if (skillRequirement == null)
            {
                return NotFound();
            }

            return View(skillRequirement);
        }

        // GET: SkillRequirements/Create
        public IActionResult Create(int? jobId)
        {
            if (jobId == null)
            {
                // Handle the case where jobId is not provided
                ModelState.AddModelError("", "JobId is required.");
                return View();
            }

            // Pass the jobId to the view
            ViewData["JobId"] = jobId;
            return View();
        }

        // POST: SkillRequirements/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("JobId,Name,Scoring")] SkillRequirement skillRequirement)
        {
            if (ModelState.IsValid)
            {
                // Check if the JobId exists in the database
                var jobExists = await _context.JobPostings.AnyAsync(j => j.JobId == skillRequirement.JobId);
                if (!jobExists)
                {
                    ModelState.AddModelError("JobId", "The selected JobId does not exist.");
                    return View(skillRequirement);
                }

                // Save the new skill requirement to the database
                _context.Add(skillRequirement);
                await _context.SaveChangesAsync();

                // Add a success message to TempData
                TempData["SuccessMessage"] = "Skill requirement added successfully!";

                // Redirect back to the same "Create" view, passing the JobId again
                return RedirectToAction(nameof(Create), new { jobId = skillRequirement.JobId });
            }
            // If the model state is invalid, redisplay the form with the current JobId
            ViewData["JobId"] = skillRequirement.JobId;
            return View(skillRequirement);
        }

        // GET: SkillRequirements/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var skillRequirement = await _context.SkillRequirements
                .FirstOrDefaultAsync(sr => sr.SkillRequirementId == id);
            //var skillRequirement = await _context.SkillRequirements.FindAsync(id);
            if (skillRequirement == null)
            {
                return NotFound();
            }
            return View(skillRequirement);
        }

        // POST: SkillRequirements/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SkillRequirement skillRequirement)
        {
            if (id != skillRequirement.SkillRequirementId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(skillRequirement);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SkillRequirementExists(skillRequirement.SkillRequirementId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                // Redirect to GetByJobId with the JobId of the updated SkillRequirement
                return RedirectToAction(nameof(GetByJobId), new { jobId = skillRequirement.JobId });
            }
            return View(skillRequirement);
        }

        // GET: SkillRequirements/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var skillRequirement = await _context.SkillRequirements
                .FirstOrDefaultAsync(m => m.SkillRequirementId == id);
            if (skillRequirement == null)
            {
                return NotFound();
            }

            return View(skillRequirement);
        }

        // POST: SkillRequirements/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var skillRequirement = await _context.SkillRequirements.FindAsync(id);
            if (skillRequirement != null)
            {
                _context.SkillRequirements.Remove(skillRequirement);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SkillRequirementExists(int id)
        {
            return _context.SkillRequirements.Any(e => e.SkillRequirementId == id);
        }
    }
}
