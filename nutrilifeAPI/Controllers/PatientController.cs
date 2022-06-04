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
    public class PatientController : ControllerBase
    {
        private readonly nutrilifeDbContext _context;

        public PatientController(nutrilifeDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [Route("Login")]
        public async Task<Patients> Login(string mail, string password)
        {
            var patient = await _context.Patients.Where(x => x.Mail == mail && x.Password == password).FirstOrDefaultAsync();

            if (patient != null)
            {
                return patient;
            }
            else
            {
                return null;
            }
        }

        [HttpGet]
        [Route("GetAllPatients")]
        public async Task<IEnumerable<Patients>> Get()
        {
            return await _context.Patients.ToListAsync();
        }

        [HttpGet]
        [Route("GetPatientsById/{id}")]
        [ProducesResponseType(typeof(Patients), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var patients = await _context.Patients.FindAsync(id);
            return patients == null ? NotFound() : Ok(patients);
        }

        [HttpGet]
        [Route("ViewDocuments/{patientId}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ViewDocuments(int patientId, DocumentType type)
        {
            var patientInfo = await _context.PatientInformations.Where(x => x.PatientId == patientId).FirstOrDefaultAsync();
            if (patientInfo == null)
                return NotFound();

            if (type == DocumentType.BloodValues)
            {
                var stream = System.IO.File.ReadAllBytes(patientInfo.BloodValues);
                if (stream == null)
                    return NotFound();
                var provider = new FileExtensionContentTypeProvider();
                if (!provider.TryGetContentType(patientInfo.BloodValues, out var contentType))
                {
                    contentType = "application/octet-stream";
                }
                return File(stream, contentType, Path.GetFileName(patientInfo.BloodValues));

            }
            else if (type == DocumentType.AnthropometricMeasurement)
            {
                var stream = System.IO.File.ReadAllBytes(patientInfo.AnthropometricMeasurement);
                if (stream == null)
                    return NotFound();
                var provider = new FileExtensionContentTypeProvider();
                if (!provider.TryGetContentType(patientInfo.AnthropometricMeasurement, out var contentType))
                {
                    contentType = "application/octet-stream";
                }
                return File(stream, contentType, Path.GetFileName(patientInfo.AnthropometricMeasurement));
            }
            else if (type == DocumentType.FatMuscleMeasurement)
            {
                var stream = System.IO.File.ReadAllBytes(patientInfo.FatMuscleMeasurement);
                if (stream == null)
                    return NotFound();
                var provider = new FileExtensionContentTypeProvider();
                if (!provider.TryGetContentType(patientInfo.FatMuscleMeasurement, out var contentType))
                {
                    contentType = "application/octet-stream";
                }
                return File(stream, contentType, Path.GetFileName(patientInfo.FatMuscleMeasurement));
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpPost]
        [Route("CreatePatient")]
        public async Task<IActionResult> Create(Patients patient)
        {
            await _context.Patients.AddAsync(patient);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = patient.Id }, patient);
        }

        [HttpPost]
        [Route("AddPatientInfo1")]
        public async Task<IActionResult> AddInfo1(int patientId, string gender, DateTime dateOfBirth, double size, double weight, string weightStatus,
            string chronicDiseases)
        {
            PatientInformations patientInfo = new PatientInformations()
            {
                Gender = gender,
                DateOfBirth = dateOfBirth,
                Size = size,
                Weight = weight,
                WeightStatus = weightStatus,
                ChronicDiseases = chronicDiseases,
                PatientId = patientId,
                BloodValues = "",
                AnthropometricMeasurement = "",
                FatMuscleMeasurement = "",
                BodyMassIndex = 0,
                SmokingStatus = false,
                AlcoholStatus = false,
            };

            _context.PatientInformations.Add(patientInfo);

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPut]
        [Route("AddPatientInfo2")]
        public async Task<IActionResult> AddInfo2(int patientId, IFormFile bloodValuees, IFormFile AnthropometricMeasurement, IFormFile FatMuscleMeasurement,
                                                  bool smokingStatus, bool AlcoholStatus)
        {
            var bloodFileName = $"{patientId}" + Path.GetFileName(bloodValuees.FileName);
            var anthFileName = $"{patientId}" + Path.GetFileName(AnthropometricMeasurement.FileName);
            var fatMuscleFileName = $"{patientId}" + Path.GetFileName(FatMuscleMeasurement.FileName);

            var bloodFilePath = Path.Combine(@"C:\temp\patientInfos\", bloodFileName);
            var anthFilePath = Path.Combine(@"C:\temp\patientInfos\", anthFileName);
            var fatMuscleFilePath = Path.Combine(@"C:\temp\patientInfos\", fatMuscleFileName);
            Directory.CreateDirectory(@"C:\temp\patientInfos\");

            using (var stream = System.IO.File.Create(bloodFilePath))
            {
                await bloodValuees.CopyToAsync(stream);
            }
            using (var stream = System.IO.File.Create(anthFilePath))
            {
                await AnthropometricMeasurement.CopyToAsync(stream);
            }
            using (var stream = System.IO.File.Create(fatMuscleFilePath))
            {
                await FatMuscleMeasurement.CopyToAsync(stream);
            }

            PatientInformations patientInfos = _context.PatientInformations.Where(x => x.PatientId == patientId).FirstOrDefault();
            if (patientInfos == null) return NotFound();

            if (patientInfos != null)
            {
                patientInfos.BloodValues = bloodFilePath;
                patientInfos.AnthropometricMeasurement = anthFilePath;
                patientInfos.FatMuscleMeasurement = fatMuscleFilePath;
                patientInfos.BodyMassIndex = patientInfos.Weight / (patientInfos.Size * patientInfos.Size);
                patientInfos.SmokingStatus = smokingStatus;
                patientInfos.AlcoholStatus = AlcoholStatus;
            }

            _context.Entry(patientInfos).State = EntityState.Modified;

            Patients patient = _context.Patients.Find(patientId);
            if (patient == null) return NotFound();

            patient.InfoId = patientInfos.Id;
            _context.Entry(patient).State = EntityState.Modified;

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPut]
        [Route("UpdatePatient/{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, Patients patient)
        {
            if (id != patient.Id) return BadRequest();

            _context.Entry(patient).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut]
        [Route("UpdatePatientInfo/{patiendId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateInfo(int patiendId, PatientInformations patientInfo)
        {
            if (patiendId != patientInfo.PatientId) return BadRequest();

            _context.Entry(patientInfo).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut]
        [Route("UpdatePatientsDocuments/{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateDocuments(int id, IFormFile document, DocumentType type)
        {
            var patientInfo = _context.PatientInformations.Where(x => x.PatientId == id).FirstOrDefault();
            if (patientInfo == null) return BadRequest();

            if (type == DocumentType.BloodValues)
            {
                var bloodFileName = $"{id}" + Path.GetFileName(document.FileName);
                var bloodFilePath = Path.Combine(@"C:\temp\patientInfos\", bloodFileName);

                System.IO.File.Delete(patientInfo.BloodValues);
                using (var stream = System.IO.File.Create(bloodFilePath))
                {
                    await document.CopyToAsync(stream);
                }

                patientInfo.BloodValues = bloodFilePath;
            }
            else if (type == DocumentType.AnthropometricMeasurement)
            {
                var anthFileName = $"{id}" + Path.GetFileName(document.FileName);
                var anthFilePath = Path.Combine(@"C:\temp\patientInfos\", anthFileName);

                System.IO.File.Delete(patientInfo.AnthropometricMeasurement);
                using (var stream = System.IO.File.Create(anthFilePath))
                {
                    await document.CopyToAsync(stream);
                }

                patientInfo.AnthropometricMeasurement = anthFilePath;
            }
            else if (type == DocumentType.FatMuscleMeasurement)
            {
                var fatMuscleFileName = $"{id}" + Path.GetFileName(document.FileName);
                var fatMuscleFilePath = Path.Combine(@"C:\temp\patientInfos\", fatMuscleFileName);

                System.IO.File.Delete(patientInfo.FatMuscleMeasurement);
                using (var stream = System.IO.File.Create(fatMuscleFilePath))
                {
                    await document.CopyToAsync(stream);
                }

                patientInfo.FatMuscleMeasurement = fatMuscleFilePath;
            }
            else
            {
                return BadRequest();
            }

            _context.Entry(patientInfo).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete]
        [Route("DeletePatient/{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var patient = await _context.Patients.FindAsync(id);
            if (patient == null) return NotFound();

            var patientInfo = await _context.PatientInformations.FindAsync(patient.InfoId);
            if (patientInfo == null) return NotFound();

            var ratings = await _context.Ratings.Where(x => x.PatientId == id).ToListAsync();
            var appointments = await _context.Appointments.Where(x => x.PatientId == id).ToListAsync();

            System.IO.File.Delete(patientInfo.FatMuscleMeasurement);
            System.IO.File.Delete(patientInfo.AnthropometricMeasurement);
            System.IO.File.Delete(patientInfo.BloodValues);

            _context.PatientInformations.Remove(patientInfo);
            foreach (var rating in ratings)
            {
                _context.Ratings.Remove(rating);
            }
            foreach (var appointment in appointments)
            {
                _context.Appointments.Remove(appointment);
            }
            await _context.SaveChangesAsync();
            _context.Patients.Remove(patient);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut]
        [Route("UpdatePatientPassword/{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<string> UpdatePassword(int id)
        {
            string newPassowrd = GeneratePassword();

            var patient = _context.Patients.Find(id);
            if (patient == null) return "Bulunamadı";

            patient.Password = newPassowrd;

            _context.Entry(patient).State = EntityState.Modified;
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
                password.Append((char)random.Next(33, 48));
            if (digit)
                password.Append((char)random.Next(48, 58));
            if (lowercase)
                password.Append((char)random.Next(97, 123));
            if (uppercase)
                password.Append((char)random.Next(65, 91));

            return password.ToString();
        }

        [HttpGet]
        [Route("SearchPatient")]
        public async Task<IEnumerable<Patients>> Search(string nameSurname)
        {
            return await _context.Patients.Where(x => x.Name.Contains(nameSurname) || x.Surname.Contains(nameSurname) || (x.Name + " " + x.Surname).Contains(nameSurname)).ToListAsync();
        }

        [HttpPost]
        [Route("GivePoints")]
        public async Task<IActionResult> GivePoints(int patientId, int nutritionistId, int rate)
        {
            var nutritionist = await _context.Nutritionists.FindAsync(nutritionistId);
            var patient = await _context.Patients.FindAsync(patientId);



            if (nutritionist != null && patient != null)
            {
                if (nutritionist.Point == 0)
                {
                    nutritionist.Point = rate;
                    _context.Entry(nutritionist).State = EntityState.Modified;

                    Ratings rating = new Ratings()
                    {
                        Rated = rate,
                        PatientId = patientId,
                        NutritionistId = nutritionistId,
                    };
                    await _context.Ratings.AddAsync(rating);

                    await _context.SaveChangesAsync();
                }
                else
                {
                    Ratings rating = new Ratings()
                    {
                        Rated = rate,
                        PatientId = patientId,
                        NutritionistId = nutritionistId,
                    };
                    await _context.Ratings.AddAsync(rating);

                    var totalPoints = _context.Ratings.Where(x => x.NutritionistId == nutritionistId).Sum(x => x.Rated);
                    var countPoints = _context.Ratings.Where(x => x.NutritionistId == nutritionistId).Count();
                    double newPoints = ((double)totalPoints + (double)rate) / ((double)countPoints + 1);

                    nutritionist.Point = newPoints;
                    _context.Entry(nutritionist).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                }
            }

            return Ok();
        }

        [HttpPost]
        [Route("SendMessageToNutritionist")]
        public async Task<string> SendMessage(int nutritionistId, int patientId, string messageContent)
        {
            var patient = await _context.Patients.FindAsync(patientId);
            Messages message = new Messages()
            {
                MessageDate = DateTime.Now,
                Sender = $"PatientId:{patientId}",
                Recipient = $"NutritionistId:{nutritionistId}",
                MessageContent = messageContent
            };

            await _context.Messages.AddAsync(message);
            await _context.SaveChangesAsync();

            return $"{patient.Name}:{messageContent}";
        }

        [HttpGet]
        [Route("ViewMessageWithNutritionist")]
        public async Task<IEnumerable<Messages>> ViewMessage(int nutritionistId, int patientId)
        {
            return await _context.Messages.Where(x => (x.Sender.EndsWith(nutritionistId.ToString()) && x.Recipient.EndsWith(patientId.ToString()))
                                                || (x.Sender.EndsWith(patientId.ToString()) && x.Recipient.EndsWith(nutritionistId.ToString()))).ToListAsync();
        }

        [HttpGet]
        [Route("GetAppointments")]
        public async Task<IEnumerable<AppointmentDTO>> GetAppointments(int patientId)
        {
            var appointments = _context.Appointments.Where(x => x.PatientId == patientId && x.Status == true).Select(y => new AppointmentDTO
            {
                AppointmentDate = y.AppointmentDate,
                Note = y.Note,
                NutritionistName = _context.Nutritionists.Where(x => x.Id == y.NutritionistId).Select(x => x.Name).FirstOrDefault()
                                    + " " + _context.Nutritionists.Where(x => x.Id == y.NutritionistId).Select(x => x.Surname).FirstOrDefault(),
                PatientName = _context.Patients.Where(x => x.Id == patientId).Select(x => x.Name).FirstOrDefault()
                                    + " " + _context.Patients.Where(x => x.Id == patientId).Select(x => x.Surname).FirstOrDefault()

            }).ToList();
            return appointments;
        }
    }
}
