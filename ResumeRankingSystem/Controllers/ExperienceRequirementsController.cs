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
    public class ExperienceRequirementsController : Controller
    {
        private readonly DatabaseDbContext _context;

        public ExperienceRequirementsController(DatabaseDbContext context)
        {
            _context = context;
        }

        // GET: ExperienceRequirements
        public async Task<IActionResult> Index()
        {
            return View(await _context.ExperienceRequirements.ToListAsync());
        }

        // Get: All Experience Requirements for a specific Job
        [HttpGet]
        public async Task<IActionResult> GetByJobId(int jobId)
        {
            // Retrieve all skill requirements associated with the specified JobId
            var experienceRequirements = await _context.ExperienceRequirements
                .Where(sr => sr.JobId == jobId)
                .ToListAsync();

            // Check if any skill requirements were found
            if (experienceRequirements == null || !experienceRequirements.Any())
            {
                return NotFound();
            }

            // Pass the jobId to the view
            ViewData["JobId"] = jobId;

            return View("ExperienceList", experienceRequirements);
        }

        // GET: ExperienceRequirements/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var experienceRequirement = await _context.ExperienceRequirements
                .FirstOrDefaultAsync(m => m.ExperienceRequirementId == id);
            if (experienceRequirement == null)
            {
                return NotFound();
            }

            return View(experienceRequirement);
        }

        // GET: ExperienceRequirements/Create
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

        // POST: ExperienceRequirements/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create([Bind("JobId,Name,Scoring")] ExperienceRequirement experienceRequirement)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _context.Add(experienceRequirement);
        //        await _context.SaveChangesAsync();
        //        return RedirectToAction(nameof(Index));
        //    }
        //    return View(experienceRequirement);
        //}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("JobId,Name,Scoring")] ExperienceRequirement experienceRequirement)
        {
            if (ModelState.IsValid)
            {
                // Check if the JobId exists in the database
                var jobExists = await _context.JobPostings.AnyAsync(j => j.JobId == experienceRequirement.JobId);
                if (!jobExists)
                {
                    ModelState.AddModelError("JobId", "The selected JobId does not exist.");
                    return View(experienceRequirement);
                }

                // Save the new education requirement to the database
                _context.Add(experienceRequirement);
                await _context.SaveChangesAsync();

                // Add a success message to TempData
                TempData["SuccessMessage"] = "Experience requirement added successfully!";

                // Redirect back to the same "Create" view, passing the JobId again
                return RedirectToAction(nameof(Create), new { jobId = experienceRequirement.JobId });
            }

            // If the model state is invalid, redisplay the form with the current JobId
            ViewBag.JobId = experienceRequirement.JobId;
            return View(experienceRequirement);
        }

        // GET: ExperienceRequirements/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var experienceRequirement = await _context.ExperienceRequirements
                .FirstOrDefaultAsync(sr => sr.ExperienceRequirementId == id);
            //var experienceRequirement = await _context.ExperienceRequirements.FindAsync(id);
            if (experienceRequirement == null)
            {
                return NotFound();
            }
            return View(experienceRequirement);
        }

        // POST: ExperienceRequirements/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ExperienceRequirement experienceRequirement)
        {
            if (id != experienceRequirement.ExperienceRequirementId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(experienceRequirement);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ExperienceRequirementExists(experienceRequirement.ExperienceRequirementId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(GetByJobId), new { jobId = experienceRequirement.JobId });
            }
            return View(experienceRequirement);
        }

        // GET: ExperienceRequirements/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var experienceRequirement = await _context.ExperienceRequirements
                .FirstOrDefaultAsync(m => m.ExperienceRequirementId == id);
            if (experienceRequirement == null)
            {
                return NotFound();
            }

            return View(experienceRequirement);
        }

        // POST: ExperienceRequirements/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var experienceRequirement = await _context.ExperienceRequirements.FindAsync(id);
            if (experienceRequirement != null)
            {
                _context.ExperienceRequirements.Remove(experienceRequirement);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ExperienceRequirementExists(int id)
        {
            return _context.ExperienceRequirements.Any(e => e.ExperienceRequirementId == id);
        }
    }
}
