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
    public class EducationRequirementsController : Controller
    {
        private readonly DatabaseDbContext _context;

        public EducationRequirementsController(DatabaseDbContext context)
        {
            _context = context;
        }

        // GET: EducationRequirements
        public async Task<IActionResult> Index()
        {
            return View(await _context.EducationRequirements.ToListAsync());
        }

        // Get: All Education Requirements for a specific Job
        [HttpGet]
        public async Task<IActionResult> GetByJobId(int jobId)
        {
            // Retrieve all skill requirements associated with the specified JobId
            var educationRequirements = await _context.EducationRequirements
                .Where(sr => sr.JobId == jobId)
                .ToListAsync();

            // Check if any skill requirements were found
            if (educationRequirements == null || !educationRequirements.Any())
            {
                return NotFound();
            }

            // Pass the jobId to the view
            ViewData["JobId"] = jobId;

            return View("EducationList", educationRequirements);
        }

        // GET: EducationRequirements/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var educationRequirement = await _context.EducationRequirements
                .FirstOrDefaultAsync(m => m.EducationRequirementId == id);
            if (educationRequirement == null)
            {
                return NotFound();
            }

            return View(educationRequirement);
        }

        // GET: EducationRequirements/Create
        public IActionResult Create(int? jobId)
        {
            if (jobId == null)
            {
                ModelState.AddModelError("", "JobId is required.");
                return View();
            }

            // Pass the jobId to the view
            ViewBag.JobId = jobId;

            return View();
        }

        // POST: EducationRequirements/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create([Bind("JobId,Name,Scoring")] EducationRequirement educationRequirement)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _context.Add(educationRequirement);
        //        await _context.SaveChangesAsync();
        //        return RedirectToAction(nameof(Index));
        //    }
        //    return View(educationRequirement);
        //}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("JobId,Name,Scoring")] EducationRequirement educationRequirement)
        {
            if (ModelState.IsValid)
            {
                // Check if the JobId exists in the database
                var jobExists = await _context.JobPostings.AnyAsync(j => j.JobId == educationRequirement.JobId);
                if (!jobExists)
                {
                    ModelState.AddModelError("JobId", "The selected JobId does not exist.");
                    return View(educationRequirement);
                }

                // Save the new education requirement to the database
                _context.Add(educationRequirement);
                await _context.SaveChangesAsync();
                  
                // Add a success message to TempData
                TempData["SuccessMessage"] = "Education requirement added successfully!";

                // Redirect back to the same "Create" view, passing the JobId again
                return RedirectToAction(nameof(Create), new { jobId = educationRequirement.JobId });
            }

            // If the model state is invalid, redisplay the form with the current JobId
            ViewBag.JobId = educationRequirement.JobId;
            return View(educationRequirement);
        }

        // GET: EducationRequirements/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var educationRequirement = await _context.EducationRequirements
                .FirstOrDefaultAsync(sr => sr.EducationRequirementId == id);
            //var educationRequirement = await _context.EducationRequirements.FindAsync(id);
            if (educationRequirement == null)
            {
                return NotFound();
            }
            return View(educationRequirement);
        }

        // POST: EducationRequirements/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EducationRequirement educationRequirement)
        {
            if (id != educationRequirement.EducationRequirementId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(educationRequirement);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EducationRequirementExists(educationRequirement.EducationRequirementId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(GetByJobId), new { jobId = educationRequirement.JobId });

            }
            return View(educationRequirement);
        }

        // GET: EducationRequirements/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var educationRequirement = await _context.EducationRequirements
                .FirstOrDefaultAsync(m => m.EducationRequirementId == id);
            if (educationRequirement == null)
            {
                return NotFound();
            }

            return View(educationRequirement);
        }

        // POST: EducationRequirements/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var educationRequirement = await _context.EducationRequirements.FindAsync(id);
            if (educationRequirement != null)
            {
                _context.EducationRequirements.Remove(educationRequirement);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EducationRequirementExists(int id)
        {
            return _context.EducationRequirements.Any(e => e.EducationRequirementId == id);
        }
    }
}
