using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using nutrilifeAPI.Common.Enums;
using nutrilifeAPI.Context;
using nutrilifeAPI.Models;
using nutrilifeAPI.Models.DTOs;
using System.Text;

namespace nutrilifeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NutritionistController : ControllerBase
    {
        private readonly nutrilifeDbContext _context;

        public NutritionistController(nutrilifeDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [Route("Login")]
        public async Task<Nutritionists> Login(string mail, string password)
        {
            var nutritionist = await _context.Nutritionists.Where(x => x.Mail == mail && x.Password == password).FirstOrDefaultAsync();

            if (nutritionist != null)
            {
                return nutritionist;
            }
            else
            {
                return null;
            }
        }

        [HttpGet]
        [Route("GetAllNutritionists")]
        public async Task<IEnumerable<Nutritionists>> Get()
        {
            return await _context.Nutritionists.ToListAsync();
        }

        [HttpGet]
        [Route("GetNutritionistById/{id}")]
        [ProducesResponseType(typeof(Nutritionists), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var nutritionist = await _context.Nutritionists.FindAsync(id);
            return nutritionist == null ? NotFound() : Ok(nutritionist);
        }

        [HttpGet]
        [Route("GetProfilePictureById/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProfilePicture(int id)
        {
            var nutritionist = await _context.Nutritionists.FindAsync(id);
            if (nutritionist == null)
                return NotFound();

            var stream = System.IO.File.ReadAllBytes(nutritionist.ProfilePicture);
            if (stream == null)
                return NotFound();

            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(nutritionist.ProfilePicture, out var contentType))
            {
                contentType = "application/octet-stream";
            }

            return File(stream, contentType, Path.GetFileName(nutritionist.ProfilePicture));
        }

        [HttpGet]
        [Route("ViewDocuments/{id}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ViewDocuments(int id, DocumentType type)
        {
            var nutritionist = await _context.Nutritionists.FindAsync(id);
            if (nutritionist == null)
                return NotFound();

            if (type == DocumentType.Cv)
            {
                var stream = System.IO.File.ReadAllBytes(nutritionist.CV);
                if (stream == null)
                    return NotFound();
                var provider = new FileExtensionContentTypeProvider();
                if (!provider.TryGetContentType(nutritionist.CV, out var contentType))
                {
                    contentType = "application/octet-stream";
                }
                return File(stream, contentType, Path.GetFileName(nutritionist.CV));

            }
            else if (type == DocumentType.Degree)
            {
                var stream = System.IO.File.ReadAllBytes(nutritionist.Degree);
                if (stream == null)
                    return NotFound();
                var provider = new FileExtensionContentTypeProvider();
                if (!provider.TryGetContentType(nutritionist.Degree, out var contentType))
                {
                    contentType = "application/octet-stream";
                }
                return File(stream, contentType, Path.GetFileName(nutritionist.Degree));
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpPost]
        [Route("CreateNutritionist")]
        public async Task<IActionResult> Create(string name, string surname, string mail, string password, string gender,
            IFormFile cv, IFormFile degree, IFormFile? profilePicture, bool weightManagement, bool sportsNutrition,
            bool pregnancyNutrition, bool diabeticNutrition, bool childNutrition, string phoneNumber)
        {
            var cvFileName = $"{name}{surname}" + Path.GetFileName(cv.FileName);
            var degreeFileName = $"{name}{surname}" + Path.GetFileName(degree.FileName);

            var cvFilePath = Path.Combine(@"C:\temp\nutritionistDocuments\", cvFileName);
            var degreeFilePath = Path.Combine(@"C:\temp\nutritionistDocuments\", degreeFileName);
            var profilePictureFilePath = "";


            Directory.CreateDirectory(@"C:\temp\nutritionistDocuments\");
            using (var stream = System.IO.File.Create(cvFilePath))
            {
                await cv.CopyToAsync(stream);
            }
            using (var stream = System.IO.File.Create(degreeFilePath))
            {
                await degree.CopyToAsync(stream);
            }

            if (profilePicture != null)
            {
                var profilePictureFileName = $"{name}{surname}" + Path.GetFileName(profilePicture.FileName);
                profilePictureFilePath = Path.Combine(@"C:\temp\nutritionistDocuments\", profilePictureFileName);
                using (var stream = System.IO.File.Create(profilePictureFilePath))
                {
                    await profilePicture.CopyToAsync(stream);
                }
            }

            Nutritionists nutritionist = new Nutritionists()
            {
                Name = name,
                Surname = surname,
                Mail = mail,
                Password = password,
                Gender = gender,
                CV = cvFilePath,
                Degree = degreeFilePath,
                Point = 0,
                WeightManagement = weightManagement,
                SportsNutrition = sportsNutrition,
                PregnancyNutrition = pregnancyNutrition,
                DiabeticNutrition = diabeticNutrition,
                ChildNutrition = childNutrition,
                ProfilePicture = profilePicture == null ? "" : profilePictureFilePath,
                PhoneNumber = phoneNumber
            };

            await _context.Nutritionists.AddAsync(nutritionist);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new
            {
                id = nutritionist.Id
            }, nutritionist);
        }

        [HttpPut]
        [Route("UpdateNutritionist/{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Update(int id, Nutritionists nutritionist)
        {
            if (id != nutritionist.Id) return BadRequest();

            _context.Entry(nutritionist).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut]
        [Route("UpdateNutritionistProfilePicture/{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateProfilePicture(int id, IFormFile profilePicture)
        {
            var nutritionist = _context.Nutritionists.Find(id);
            if (nutritionist == null) return BadRequest();

            var pictureFileName = $"{nutritionist.Name}{id}" + Path.GetFileName(profilePicture.FileName);
            var pictureFilePath = Path.Combine(@"C:\temp\nutritionistDocuments\", pictureFileName);

            if (System.IO.File.Exists(nutritionist.ProfilePicture))
                System.IO.File.Delete(nutritionist.ProfilePicture);
            using (var stream = System.IO.File.Create(pictureFilePath))
            {
                await profilePicture.CopyToAsync(stream);
            }

            nutritionist.ProfilePicture = pictureFilePath;


            _context.Entry(nutritionist).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut]
        [Route("UpdateNutritionistDocuments/{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateDocuments(int id, IFormFile document, DocumentType type)
        {
            var nutritionist = _context.Nutritionists.Find(id);
            if (nutritionist == null) return BadRequest();

            if (type == DocumentType.Cv)
            {
                var cvFileName = $"{nutritionist.Name}{nutritionist.Surname}" + Path.GetFileName(document.FileName);
                var cvFilePath = Path.Combine(@"C:\temp\nutritionistDocuments\", cvFileName);

                System.IO.File.Delete(nutritionist.CV);
                using (var stream = System.IO.File.Create(cvFilePath))
                {
                    await document.CopyToAsync(stream);
                }

                nutritionist.CV = cvFilePath;
            }
            else if (type == DocumentType.Degree)
            {
                var degreeFileName = $"{nutritionist.Name}{nutritionist.Surname}" + Path.GetFileName(document.FileName);
                var degreeFilePath = Path.Combine(@"C:\temp\nutritionistDocuments\", degreeFileName);

                System.IO.File.Delete(nutritionist.Degree);
                using (var stream = System.IO.File.Create(degreeFilePath))
                {
                    await document.CopyToAsync(stream);
                }

                nutritionist.Degree = degreeFilePath;
            }
            else
            {
                return BadRequest();
            }

            _context.Entry(nutritionist).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete]
        [Route("DeleteNutritionist/{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var nutritionist = await _context.Nutritionists.FindAsync(id);
            if (nutritionist == null)
                return NotFound();

            System.IO.File.Delete(nutritionist.ProfilePicture);
            System.IO.File.Delete(nutritionist.CV);
            System.IO.File.Delete(nutritionist.Degree);

            var ratings = _context.Ratings.Where(x => x.NutritionistId == id).ToList();
            var recipes = _context.Recipes.Where(x => x.NutritionistId == id).ToList();
            var appointments = _context.Appointments.Where(x => x.NutritionistId == id).ToList();



            foreach (var rating in ratings)
            {
                _context.Ratings.Remove(rating);
            }
            foreach (var recipe in recipes)
            {
                _context.Recipes.Remove(recipe);
            }
            foreach (var appointment in appointments)
            {
                _context.Appointments.Remove(appointment);
            }
            await _context.SaveChangesAsync();

            _context.Nutritionists.Remove(nutritionist);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost]
        [Route("CreateRecipe")]
        public async Task<IActionResult> CreateRecipe(int nutritionistId, string name, string materials, string explanation, IFormFile picture)
        {
            var pictureFileName = $"{name}{nutritionistId}" + Path.GetFileName(picture.FileName);
            var pictureFilePath = Path.Combine(@"C:\temp\recipes\", pictureFileName);
            Directory.CreateDirectory(@"C:\temp\recipes\");
            using (var stream = System.IO.File.Create(pictureFilePath))
            {
                await picture.CopyToAsync(stream);
            }

            Recipes recipe = new Recipes()
            {
                Name = name,
                Materials = materials,
                Explanation = explanation,
                NutritionistId = nutritionistId,
                Picture = pictureFilePath
            };

            await _context.Recipes.AddAsync(recipe);
            await _context.SaveChangesAsync();

            return Ok(recipe);
        }

        [HttpGet]
        [Route("GetAllRecipes")]
        public async Task<IEnumerable<Recipes>> GetRecipes(int nutritionistId)
        {
            return await _context.Recipes.Where(x => x.NutritionistId == nutritionistId).ToListAsync();
        }

        [HttpGet]
        [Route("GetRecipeById/{id}")]
        [ProducesResponseType(typeof(Nutritionists), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetRecipeById(int id)
        {
            var recipe = await _context.Recipes.FindAsync(id);
            return recipe == null ? NotFound() : Ok(recipe);
        }

        [HttpGet]
        [Route("GetRecipePhoto/{id}")]
        [ProducesResponseType(typeof(Nutritionists), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetRecipePhoto(int id)
        {
            var recipe = await _context.Recipes.FindAsync(id);
            if (recipe == null) return NotFound();

            var stream = System.IO.File.ReadAllBytes(recipe.Picture);
            if (stream == null) return NotFound();

            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(recipe.Picture, out var contentType))
            {
                contentType = "application/octet-stream";
            }

            return File(stream, contentType, Path.GetFileName(recipe.Picture));
        }

        [HttpDelete]
        [Route("DeleteRecipe/{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteRecipe(int id)
        {
            var recipe = await _context.Recipes.FindAsync(id);
            if (recipe == null) return NotFound();

            System.IO.File.Delete(recipe.Picture);
            _context.Recipes.Remove(recipe);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut]
        [Route("UpdateNutritionistPassword/{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<string> UpdatePassword(int id)
        {
            string newPassowrd = GeneratePassword();

            var nutritionist = _context.Nutritionists.Find(id);
            if (nutritionist == null) return "Bulunamadı";

            nutritionist.Password = newPassowrd;

            _context.Entry(nutritionist).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return newPassowrd;
        }

        private string GeneratePassword()
        {
            bool nonAlphanumeric = true;
            bool digit = true;
            bool lowercase = true;
            bool uppercase = true;

            StringBuilder password = new StringBuilder();
            Random random = new Random();

            while (password.Length < 12)
            {
                char c = (char)random.Next(32, 126);

                password.Append(c);

                if (char.IsDigit(c))
                    digit = false;
                else if (char.IsLower(c))
                    lowercase = false;
                else if (char.IsUpper(c))
                    uppercase = false;
                else if (!char.IsLetterOrDigit(c))
                    nonAlphanumeric = false;
            }

            if (nonAlphanumeric)
                password.Append((char)random.Next(33, 48)); //special characters
            if (digit)
                password.Append((char)random.Next(48, 58)); //numbers
            if (lowercase)
                password.Append((char)random.Next(97, 123)); //lowercase characters
            if (uppercase)
                password.Append((char)random.Next(65, 91));  //uppercase characters

            return password.ToString();
        }

        [HttpGet]
        [Route("SearchNutritionist")]
        public async Task<IEnumerable<Nutritionists>> Search(string nameSurname)
        {
            return await _context.Nutritionists.Where(x => x.Name.Contains(nameSurname) || x.Surname.Contains(nameSurname) || (x.Name + " " + x.Surname).Contains(nameSurname)).ToListAsync();
        }

        [HttpPost]
        [Route("SendMessageToPatient")]
        public async Task<string> SendMessage(int nutritionistId, int patientId, string messageContent)
        {
            var nutritionist = await _context.Nutritionists.FindAsync(nutritionistId);
            Messages message = new Messages()
            {
                MessageDate = DateTime.Now,
                Sender = $"NutritionistId:{nutritionistId}",
                Recipient = $"PatientId:{patientId}",
                MessageContent = messageContent
            };

            await _context.Messages.AddAsync(message);
            await _context.SaveChangesAsync();

            return $"{nutritionist.Name}:{messageContent}";
        }

        [HttpGet]
        [Route("ViewMessageWithPatient")]
        public async Task<IEnumerable<Messages>> ViewMessage(int nutritionistId, int patientId)
        {
            return await _context.Messages.Where(x => (x.Sender.EndsWith(nutritionistId.ToString()) && x.Recipient.EndsWith(patientId.ToString()))
                                                || (x.Sender.EndsWith(patientId.ToString()) && x.Recipient.EndsWith(nutritionistId.ToString()))).ToListAsync();
        }

        [HttpPost]
        [Route("AddPatient")]
        public async Task<int> AddPatient(int nutritionistId, int patientId)
        {
            NutritionistHavingPatient havingPatient = new NutritionistHavingPatient()
            {
                NutritionistId = nutritionistId,
                PatientId = patientId
            };

            await _context.NutritionistHavingPatient.AddAsync(havingPatient);
            return await _context.SaveChangesAsync();
        }

        [HttpGet]
        [Route("GetPatients")]
        public async Task<IEnumerable<string>> GetPatient(int nutritionistId)
        {
            List<string> patientNames = new List<string>();

            var patients = await _context.NutritionistHavingPatient.Where(x => x.NutritionistId == nutritionistId).ToListAsync();
            foreach (var patient in patients)
            {
                var patientName = _context.Patients.Where(x => x.Id == patient.PatientId).FirstOrDefault();
                patientNames.Add($"{patientName.Name} {patientName.Surname}");
            }

            return patientNames;
        }

        [HttpPost]
        [Route("CreateAppointment")]
        public async Task<int> CreateAppointment(Appointments appointment)
        {
            await _context.Appointments.AddAsync(appointment);
            return await _context.SaveChangesAsync();
        }

        [HttpGet]
        [Route("GetAppointments")]
        public async Task<IEnumerable<AppointmentDTO>> GetAppointments(int nutritionistId)
        {
            var appointments = _context.Appointments.Where(x => x.NutritionistId == nutritionistId && x.Status == true).Select(y => new AppointmentDTO
            {
                AppointmentDate = y.AppointmentDate,
                Note = y.Note,
                NutritionistName = _context.Nutritionists.Where(x => x.Id == nutritionistId).Select(x => x.Name).FirstOrDefault()
                                    + " " + _context.Nutritionists.Where(x => x.Id == nutritionistId).Select(x => x.Surname).FirstOrDefault(),
                PatientName = _context.Patients.Where(x => x.Id == y.PatientId).Select(x => x.Name).FirstOrDefault()
                                    + " " + _context.Patients.Where(x => x.Id == y.PatientId).Select(x => x.Surname).FirstOrDefault()

            }).ToList();
            return appointments;
        }

        [HttpPost]
        [Route("CreatePatient")]
        public async Task<IActionResult> CreateNotificationMessage(NotificationMessages notification)
        {
            await _context.NotificationMessages.AddAsync(notification);
            await _context.SaveChangesAsync();

            return Ok(notification);
        }
    }
}
