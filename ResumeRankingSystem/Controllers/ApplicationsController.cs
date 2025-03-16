using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Domain.DataAccess;
using Domain.Entities;
using ResumeRankingLibrary.Services;

namespace ResumeRankingSystem.Controllers
{
    public class ApplicationsController : Controller
    {
        private readonly DatabaseDbContext _context;
        private readonly ResumeRanker _resumeRanker; // Declare _resumeRanker

        public ApplicationsController(DatabaseDbContext context, HttpClient httpClient)
        {
            _context = context;
            _resumeRanker = new ResumeRanker(httpClient); // Initialize _resumeRanker
        }

        // GET: Applications
        public async Task<IActionResult> Index()
        {
            var databaseDbContext = _context.Applications.Include(a => a.Applicant);
            return View(await databaseDbContext.ToListAsync());
        }

        // GET: Applications/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var application = await _context.Applications
                .Include(a => a.Applicant)
                .FirstOrDefaultAsync(m => m.ApplicationId == id);
            if (application == null)
            {
                return NotFound();
            }

            return View(application);
        }

        // GET: Applications/ByJobId/{jobId}
        [HttpGet]
        public async Task<IActionResult> ApplicantRank(int jobId)
        {
            if (jobId <= 0)
            {
                return BadRequest("Invalid JobId.");
            }

            // Query the database for applications with the specified JobId
            var applications = await _context.Applications
                .Include(a => a.Applicant) // Include related Applicant data
                .Where(a => a.JobId == jobId) // Filter by JobId
                .OrderByDescending(a => a.Score) // Sort by Scoring in descending order
                .ToListAsync();

            //if (applications == null || applications.Count == 0)
            //{
            //    return NotFound($"No applications found for JobId {jobId}.");
            //}

            // Pass the jobId to the view
            ViewBag.JobId = jobId;

            return View(applications); // Return the list of applications as JSON
        }

        // NEW ACTION: Rank Applicant Against Job Posting
        //[HttpGet]
        //public async Task<IActionResult> RankApplicant(int applicantId, int jobId)
        //{
        //    // Fetch the applicant and job posting from the database
        //    var applicant = await _context.Applicants.FindAsync(applicantId);
        //    var jobPosting = _context.JobPostings
        //        .Include(jp => jp.SkillRequirements)
        //        .Include(jp => jp.EducationRequirements)
        //        .Include(jp => jp.ExperienceRequirements)
        //        .FirstOrDefault(jp => jp.JobId == jobId);


        //    if (applicant == null || jobPosting == null)
        //    {
        //        return NotFound("Applicant or Job Posting not found.");
        //    }

        //    // Use ResumeRanker to compute the score
        //    double rankScore = _resumeRanker.ScoreApplicant(applicant, jobPosting);

        //    // Optionally, update the rank in the database
        //    var application = await _context.Applications
        //        .FirstOrDefaultAsync(a => a.ApplicantId == applicantId && a.JobId == jobId);

        //    if (application != null)
        //    {
        //        application.Scoring = rankScore; // Update the rank
        //        _context.Update(application);
        //        await _context.SaveChangesAsync();
        //    }

        //    // Return the rank score as JSON
        //    return Json(new { RankScore = rankScore });
        //}
        [HttpPost]
        public async Task<IActionResult> RankApplicant([FromForm] RankApplicantRequest request)
        {
            // Extract parameters from the request body
            var applicantId = request.ApplicantId;
            var jobId = request.JobId;

            var applicant = await _context.Applicants.FindAsync(applicantId);
            var jobPosting = await _context.JobPostings
                .Include(jp => jp.SkillRequirements)
                .Include(jp => jp.EducationRequirements)
                .Include(jp => jp.ExperienceRequirements)
                .FirstOrDefaultAsync(jp => jp.JobId == jobId);

            if (applicant == null || jobPosting == null)
            {
                return Json(new { success = false, message = "Applicant or Job Posting not found." });
            }

            // Call ScoreApplicant with extracted data
            var (skillsScoring, educationScoring, experienceScoring, score) = _resumeRanker.ScoreApplicant(
                applicant.Skills ?? string.Empty,
                applicant.Education ?? string.Empty,
                applicant.Experience ?? string.Empty,
                jobPosting.SkillRequirements, // Pass the full SkillRequirements collection
                jobPosting.EducationRequirements, // Pass the full EducationRequirements collection
                jobPosting.ExperienceRequirements // Pass the full ExperienceRequirements collection
            );

            // Find or create the application
            var application = await _context.Applications
                .FirstOrDefaultAsync(a => a.ApplicantId == applicantId && a.JobId == jobId);

            if (application != null)
            {
                // Update existing application
                application.SkillsScoring = skillsScoring;
                application.EducationScoring = educationScoring;
                application.ExperienceScoring = experienceScoring;
                application.Score = score;
                _context.Update(application);
            }
            else
            {
                // Create new application
                application = new Application
                {
                    JobId = jobId,
                    ApplicantId = applicantId,
                    Status = "Pending", // Set default status
                    SkillsScoring = skillsScoring,
                    EducationScoring = educationScoring,
                    ExperienceScoring = experienceScoring,
                    Score = score,
                    JobPosting = jobPosting, // Attach fetched entities
                    Applicant = applicant
                };
                _context.Applications.Add(application);
            }

            await _context.SaveChangesAsync();

            // Return a success response with a message
            return Json(new { success = true, message = "Application submitted successfully!" });
        }

        [HttpPost]
        public async Task<IActionResult> RescoreApplicants(int jobId)
        {
            if (jobId <= 0)
            {
                return BadRequest("Invalid JobId.");
            }

            // Fetch all applications for the given JobId
            var applications = await _context.Applications
                .Include(a => a.Applicant) // Include related Applicant data
                .Include(a => a.JobPosting) // Include related JobPosting data
                .Where(a => a.JobId == jobId)
                .ToListAsync();

            if (applications == null || !applications.Any())
            {
                return NotFound($"No applications found for JobId {jobId}.");
            }

            // Fetch the job posting details
            var jobPosting = await _context.JobPostings
                .Include(jp => jp.SkillRequirements)
                .Include(jp => jp.EducationRequirements)
                .Include(jp => jp.ExperienceRequirements)
                .FirstOrDefaultAsync(jp => jp.JobId == jobId);

            if (jobPosting == null)
            {
                return NotFound("Job Posting not found.");
            }

            // Rescore each application
            foreach (var application in applications)
            {
                var applicant = application.Applicant;

                // Call ScoreApplicant with extracted data
                var (skillsScoring, educationScoring, experienceScoring, score) =  _resumeRanker.ScoreApplicant(
                    applicant.Skills ?? string.Empty,
                    applicant.Education ?? string.Empty,
                    applicant.Experience ?? string.Empty,
                    jobPosting.SkillRequirements, // Pass the full SkillRequirements collection
                    jobPosting.EducationRequirements, // Pass the full EducationRequirements collection
                    jobPosting.ExperienceRequirements // Pass the full ExperienceRequirements collection
                );

                // Update scoring in the application
                application.SkillsScoring = skillsScoring;
                application.EducationScoring = educationScoring;
                application.ExperienceScoring = experienceScoring;
                application.Score = score;
                _context.Update(application);
            }

            // Save changes to the database
            await _context.SaveChangesAsync();

            // Redirect back to the ApplicantRank view
            return RedirectToAction(nameof(ApplicantRank), new { jobId });
        }

        // GET: Applications/Create
        public IActionResult Create()
        {
            ViewData["ApplicantId"] = new SelectList(_context.Applicants, "ApplicantId", "Password");
            return View();
        }

        // POST: Applications/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("JobId,ApplicantId,Status,Rank")] Application application)
        {
            if (ModelState.IsValid)
            {
                _context.Add(application);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ApplicantId"] = new SelectList(_context.Applicants, "ApplicantId", "Password", application.ApplicantId);
            return View(application);
        }

        // GET: Applications/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var application = await _context.Applications.FindAsync(id);
            if (application == null)
            {
                return NotFound();
            }
            ViewData["ApplicantId"] = new SelectList(_context.Applicants, "ApplicantId", "Password", application.ApplicantId);
            return View(application);
        }

        // POST: Applications/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,Application application)
        {
            if (id != application.ApplicationId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(application);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ApplicationExists(application.ApplicationId))
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
            ViewData["ApplicantId"] = new SelectList(_context.Applicants, "ApplicantId", "Password", application.ApplicantId);
            return View(application);
        }

        // DELETE: Applications/Delete/5
        [HttpGet, ActionName("Delete")]
        public async Task<IActionResult> Delete(int id)
        {
            var application = await _context.Applications.FindAsync(id);
            if (application == null)
            {
                return NotFound();
            }

            // Retrieve the jobId before deleting the application
            int jobId = application.JobId;

            // Delete the application
            _context.Applications.Remove(application);
            await _context.SaveChangesAsync();


            // Redirect back to the ApplicantRank page with the jobId
            return RedirectToAction(nameof(ApplicantRank), new { jobId });
        }

        // GET: Applications/Delete/5
        //public async Task<IActionResult> Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var application = await _context.Applications
        //        .Include(a => a.Applicant)
        //        .FirstOrDefaultAsync(m => m.ApplicationId == id);
        //    if (application == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(application);
        //}

        //// POST: Applications/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed(int id)
        //{
        //    var application = await _context.Applications.FindAsync(id);
        //    if (application != null)
        //    {
        //        _context.Applications.Remove(application);
        //    }

        //    await _context.SaveChangesAsync();
        //    return RedirectToAction(nameof(Index));
        //}

        private bool ApplicationExists(int id)
        {
            return _context.Applications.Any(e => e.ApplicationId == id);
        }
    }
}
