﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json.Linq;
using UcbBack.Logic;
using UcbBack.Logic.ExcelFiles;
using UcbBack.Models;
using UcbBack.Models.Not_Mapped.CustomDataAnnotations;
using UcbBack.Models.ViewMoldes;

namespace UcbBack.Controllers
{
    public class AltasContratosController : ApiController
    {
        private ApplicationDbContext _context;

        public AltasContratosController()
        {
            _context = new ApplicationDbContext();
        }

        // Transform Http Content to varialbes to use in excel
        [NonAction]
        public async Task<System.Dynamic.ExpandoObject> HttpContentToVariables(MultipartMemoryStreamProvider req)
        {
            dynamic res = new System.Dynamic.ExpandoObject();
            foreach (HttpContent contentPart in req.Contents)
            {
                var contentDisposition = contentPart.Headers.ContentDisposition;
                string varname = contentDisposition.Name;
                if (varname == "\"BranchesId\"")
                {
                    res.BranchesId = contentPart.ReadAsStringAsync().Result;
                }
                else if (varname == "\"file\"")
                {
                    Stream stream = await contentPart.ReadAsStreamAsync();
                    res.fileName = String.IsNullOrEmpty(contentDisposition.FileName) ? "" : contentDisposition.FileName.Trim('"');
                    res.excelStream = stream;
                }
                else if (varname == "\"startDate\"")
                {
                    res.startDate = contentPart.ReadAsStringAsync().Result;
                }
                else if (varname == "\"endDate\"")
                {
                    res.endDate = contentPart.ReadAsStringAsync().Result;
                }
            }
            return res;
        }

        // save data in a contract from a temp alta
        [HttpGet]
        [Route("api/ContractAltaExcelsave/{id}")]
        public IHttpActionResult saveLastAltaExcel(int id)
        {
            var tempAlta = _context.TempAltas.Where(x => x.BranchesId == id && x.State == "UPLOADED");
            if (tempAlta.Count() < 0)
                return NotFound();

            foreach (var alta in tempAlta)
            {
                var person = new People();
                person = _context.Person.FirstOrDefault(x => x.CUNI == alta.CUNI);
                var contract = new ContractDetail();

                contract.Id = ContractDetail.GetNextId(_context);
                contract.DependencyId = _context.Dependencies.FirstOrDefault(x => x.Cod == alta.Dependencia).Id;
                contract.CUNI = person.CUNI;
                contract.PeopleId = person.Id;
                contract.BranchesId = alta.BranchesId;
                contract.Dedication = "TH";
                contract.Linkage = 3;
                contract.PositionDescription = "Docente Tiempo Horario";
                contract.PositionsId = 26;
                contract.StartDate = alta.StartDate;
                contract.EndDate = alta.EndDate;
                _context.ContractDetails.Add(contract);
            }

            _context.SaveChanges();
            return Ok(tempAlta);
        }

        // delete data from temp alta
        [HttpDelete]
        [Route("api/ContractAltaExcel")]
        public IHttpActionResult removeLastAltaExcel(JObject data)
        {
            int branchesid;
            if (data["segmentoOrigen"] == null || !Int32.TryParse(data["segmentoOrigen"].ToString(), out branchesid))
            {
                ModelState.AddModelError("Mal Formato", "Debes enviar mes, gestion y segmentoOrigen");
                return BadRequest();

            }
            List<TempAlta> tempAlta = _context.TempAltas.Where(x => x.BranchesId == branchesid && x.State != "UPLOADED" && x.State != "CANCELED").ToList();
            foreach (var al in tempAlta)
            {
                al.State = "CANCELED";
            }

            _context.SaveChanges();
            return Ok();
        }

        // get data from alta by branches
        [HttpGet]
        [Route("api/ContractAltaExcel/{id}")]
        public IHttpActionResult getLastAltaExcel(int id)
        {
            //todo set validators state and branchId
            string query = "select 0 as \"Id\", " +
                           " person.cuni, " +
                           "person.\"Document\", " +
                           "" + CustomSchema.Schema + ".clean_text(   " +
                           "    concat(coalesce(person.\"FirstSurName\",''),  " +
                           "concat(' ',  " +
                           "    concat(case when person.\"UseSecondSurName\"=1 then coalesce(person.\"SecondSurName\",'') else '' end,  " +
                           "concat(' ',  " +
                           "    concat( case when person.\"UseMariedSurName\"=1 then concat(coalesce(person.\"MariedSurName\",''),' ') else '' end, coalesce(person.\"Names\",''))  " +
                           "    )  " +
                           "    )  " +
                           "    )  " +
                           "    )   " +
                           "    ) as \"FullName\",   " +
                           "dep.\"Name\" as \"Dependency\", " +
                           "dep.\"Cod\" as \"DependencyCod\", " +
                           "br.\"Name\" as \"Branches\", " +
                           "br.\"Id\" as \"BranchesId\", " +
                           "'DOCENTE TIEMPO HORARIO' as \"Positions\", " +
                           "'TH' as \"Dedication\", " +
                           "'TIEMPO HORARIO' as \"Linkage\", " +
                           "TO_VARCHAR(to_date(temp.\"StartDate\"), 'DD/MM/YYYY') as \"StartDate\", " +
                           "TO_VARCHAR(to_date(temp.\"EndDate\"), 'DD/MM/YYYY') as \"EndDate\" " +
                           "from " + CustomSchema.Schema + ".\"TempAlta\" temp " +
                           "    inner join " + CustomSchema.Schema + ".\"People\" person " +
                           "    on person.cuni=temp.cuni " +
                           "inner join " + CustomSchema.Schema + ".\"Dependency\" dep " +
                           "    on dep.\"Cod\" = temp.\"Dependencia\" " +
                           "inner join " + CustomSchema.Schema + ".\"Branches\" br " +
                           "    on br.\"Id\" = temp.\"BranchesId\" " +
                           " where temp.\"BranchesId\" = " + id +
                           " and temp.\"State\" = 'UPLOADED'";
            List<ContractDetailViewModel> tempAlta = _context.Database.SqlQuery<ContractDetailViewModel>(query).ToList();
            return Ok(tempAlta);
        }

        // get Alta excel template
        [HttpGet]
        [Route("api/ContractAltaExcel/")]
        public HttpResponseMessage getAltaExcelTemplate()
        {
            ContractExcel contractExcel = new ContractExcel(fileName: "AltaExcel_TH.xlsx", headerin: 3);
            return contractExcel.getTemplate();
        }

        // upload excel alta to temp table
        [HttpPost]
        [Route("api/ContractAltaExcel/")]
        public async Task<HttpResponseMessage> AltaExcel()
        {
            var response = new HttpResponseMessage();
            try
            {
                var req = await Request.Content.ReadAsMultipartAsync();
                dynamic o = HttpContentToVariables(req).Result;
                int segment = 0;
                //validate Branches
                if (o.BranchesId == null || !Int32.TryParse(o.BranchesId, out segment))
                {
                    response.StatusCode = HttpStatusCode.BadRequest;
                    response.Content = new StringContent("Debe enviar BranchesId");
                    return response;
                }

                var segId = _context.Branch.FirstOrDefault(b => b.Id == segment);
                if (segId == null)
                {
                    response.StatusCode = HttpStatusCode.BadRequest;
                    response.Content = new StringContent("Debe enviar BranchesId valido");
                    return response;
                }
                //validate startDate of alta
                DateTime startDate = new DateTime();
                if (o.startDate == null || !DateTime.TryParse(o.startDate, out startDate))
                {
                    response.StatusCode = HttpStatusCode.BadRequest;
                    response.Content = new StringContent("Debe enviar startDate");
                    return response;
                }

                //validate endDate of alta
                DateTime endDate = new DateTime();
                if (o.endDate == null || !DateTime.TryParse(o.endDate, out endDate))
                {
                    response.StatusCode = HttpStatusCode.BadRequest;
                    response.Content = new StringContent("Debe enviar endDate");
                    return response;
                }

                if (endDate < startDate)
                {
                    response.StatusCode = HttpStatusCode.BadRequest;
                    response.Content = new StringContent("La fecha de fin no puede ser menor a la fecha de inicio");
                    return response;
                }

                var file = o.excelStream;
                var fileName = o.fileName;

                ContractExcel contractExcel = new ContractExcel(o.excelStream, _context, o.fileName, segId.Id,startDate:startDate,endDate:endDate, headerin: 3, sheets: 1);
                if (contractExcel.ValidateFile())
                {
                    string query = "update " + CustomSchema.Schema +
                                   ".\"TempAlta\" set \"State\"='CANCELED' where \"State\"='UPLOADED' and \"BranchesId\" = " +
                                   segId.Id;
                    _context.Database.ExecuteSqlCommand(query);
                    contractExcel.toDataBase();
                    response.StatusCode = HttpStatusCode.OK;
                    response.Content = new StringContent("Se subio el archivo correctamente.");
                    return response;
                }
                return contractExcel.toResponse();
            }
            catch (System.ArgumentException e)
            {
                response.StatusCode = HttpStatusCode.BadRequest;
                response.Content = new StringContent("Por favor enviar un archivo en formato excel (.xls, .xslx)" + e);
                return response;
            }
        }
    }
}
