using AutoMapper;
using iTextSharp.text.pdf;
using iTextSharp.text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaBiblioteca.Server.Models;
using SistemaBiblioteca.Server.Repositorio.Contrato;
using SistemaBiblioteca.Server.Repositorio.Implementacion;
using SistemaBiblioteca.Shared;

namespace SistemaBiblioteca.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PrestamoController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IPrestamoRepositorio _prestamoRepositorio;
        public PrestamoController(IPrestamoRepositorio prestamoRepositorio, IMapper mapper)
        {
            _mapper = mapper;
            _prestamoRepositorio = prestamoRepositorio;
        }

        [HttpGet]
        [Route("Buscar")]
        public async Task<IActionResult> Buscar(string estadoPrestamo,string codigoLector)
        {
            ResponseDTO<List<PrestamoDTO>> _ResponseDTO = new ResponseDTO<List<PrestamoDTO>>();

            try
            {
                List<PrestamoDTO> listaPrestamo = new List<PrestamoDTO>();
                IQueryable<Prestamo> query = await _prestamoRepositorio.Consultar(
                    p => p.IdEstadoPrestamoNavigation.Descripcion.ToLower().Equals(
                        estadoPrestamo.ToLower() == "todos" ? p.IdEstadoPrestamoNavigation.Descripcion.ToLower() : estadoPrestamo.ToLower())
                    &&
                    p.IdLectorNavigation.Codigo.ToLower().Equals(
                            codigoLector == "na" ? p.IdLectorNavigation.Codigo.ToLower() : codigoLector.ToLower())
                    );

                query = query.Include(e => e.IdEstadoPrestamoNavigation)
                    .Include(lt => lt.IdLectorNavigation)
                    .Include(lb => lb.IdLibroNavigation);

                listaPrestamo = _mapper.Map<List<PrestamoDTO>>(query.ToList());

                _ResponseDTO = new ResponseDTO<List<PrestamoDTO>>() { status = true, msg = "ok", value = listaPrestamo };

                return StatusCode(StatusCodes.Status200OK, _ResponseDTO);
            }
            catch (Exception ex)
            {
                _ResponseDTO = new ResponseDTO<List<PrestamoDTO>>() { status = false, msg = ex.Message, value = null };
                return StatusCode(StatusCodes.Status500InternalServerError, _ResponseDTO);
            }
        }

        [HttpGet]
        [Route("Lista")]
        public async Task<IActionResult> Lista()
        {
            ResponseDTO<List<PrestamoDTO>> _ResponseDTO = new ResponseDTO<List<PrestamoDTO>>();

            try
            {
                List<PrestamoDTO> listaPrestamo = new List<PrestamoDTO>();
                IQueryable<Prestamo> query = await _prestamoRepositorio.Consultar();
                query = query.Include(e => e.IdEstadoPrestamoNavigation)
                    .Include(lt => lt.IdLectorNavigation)
                    .Include(lb => lb.IdLibroNavigation);

                listaPrestamo = _mapper.Map<List<PrestamoDTO>>(query.ToList());

                _ResponseDTO = new ResponseDTO<List<PrestamoDTO>>() { status = true, msg = "ok", value = listaPrestamo };

                return StatusCode(StatusCodes.Status200OK, _ResponseDTO);
            }
            catch (Exception ex)
            {
                _ResponseDTO = new ResponseDTO<List<PrestamoDTO>>() { status = false, msg = ex.Message, value = null };
                return StatusCode(StatusCodes.Status500InternalServerError, _ResponseDTO);
            }
        }

        [HttpPost]
        [Route("Guardar")]
        public async Task<IActionResult> Guardar([FromBody] PrestamoDTO request)
        {
            ResponseDTO<PrestamoDTO> _ResponseDTO = new ResponseDTO<PrestamoDTO>();
            try
            {
                Prestamo _prestamo = _mapper.Map<Prestamo>(request);

                Prestamo _prestamoCreado = await _prestamoRepositorio.Crear(_prestamo);

                if (_prestamoCreado.IdLector != 0)
                    _ResponseDTO = new ResponseDTO<PrestamoDTO>() { status = true, msg = "ok", value = _mapper.Map<PrestamoDTO>(_prestamoCreado) };
                else
                    _ResponseDTO = new ResponseDTO<PrestamoDTO>() { status = false, msg = "No se pudo crear el prestamo" };

                return StatusCode(StatusCodes.Status200OK, _ResponseDTO);
            }
            catch (Exception ex)
            {
                _ResponseDTO = new ResponseDTO<PrestamoDTO>() { status = false, msg = ex.Message };
                return StatusCode(StatusCodes.Status500InternalServerError, _ResponseDTO);
            }
        }


        [HttpPut]
        [Route("Editar")]
        public async Task<IActionResult> Editar([FromBody] PrestamoDTO request)
        {
            ResponseDTO<PrestamoDTO> _ResponseDTO = new ResponseDTO<PrestamoDTO>();
            try
            {
                Prestamo _prestamo = _mapper.Map<Prestamo>(request);
                Prestamo _prestamoParaEditar = await _prestamoRepositorio.Obtener(u => u.IdPrestamo == _prestamo.IdPrestamo);

                if (_prestamoParaEditar != null)
                {

                    _prestamoParaEditar.IdEstadoPrestamo = _prestamo.IdEstadoPrestamo;
                    _prestamoParaEditar.FechaConfirmacionDevolucion = _prestamo.FechaConfirmacionDevolucion;
                    _prestamoParaEditar.EstadoRecibido = _prestamo.EstadoRecibido;

                    bool respuesta = await _prestamoRepositorio.Editar(_prestamoParaEditar);

                    if (respuesta)
                        _ResponseDTO = new ResponseDTO<PrestamoDTO>() { status = true, msg = "ok", value = _mapper.Map<PrestamoDTO>(_prestamoParaEditar) };
                    else
                        _ResponseDTO = new ResponseDTO<PrestamoDTO>() { status = false, msg = "No se pudo editar el prestamo" };
                }
                else
                {
                    _ResponseDTO = new ResponseDTO<PrestamoDTO>() { status = false, msg = "No se encontró el prestamo" };
                }

                return StatusCode(StatusCodes.Status200OK, _ResponseDTO);
            }
            catch (Exception ex)
            {
                _ResponseDTO = new ResponseDTO<PrestamoDTO>() { status = false, msg = ex.Message };
                return StatusCode(StatusCodes.Status500InternalServerError, _ResponseDTO);
            }
        }

        [HttpDelete]
        [Route("Eliminar/{id:int}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            ResponseDTO<string> _ResponseDTO = new ResponseDTO<string>();
            try
            {
                Prestamo _prestamoEliminar = await _prestamoRepositorio.Obtener(u => u.IdLector == id);

                if (_prestamoEliminar != null)
                {

                    bool respuesta = await _prestamoRepositorio.Eliminar(_prestamoEliminar);

                    if (respuesta)
                        _ResponseDTO = new ResponseDTO<string>() { status = true, msg = "ok", value = "" };
                    else
                        _ResponseDTO = new ResponseDTO<string>() { status = false, msg = "No se pudo eliminar el lector", value = "" };
                }

                return StatusCode(StatusCodes.Status200OK, _ResponseDTO);
            }
            catch (Exception ex)
            {
                _ResponseDTO = new ResponseDTO<string>() { status = false, msg = ex.Message };
                return StatusCode(StatusCodes.Status500InternalServerError, _ResponseDTO);
            }
        }

        [HttpGet("exportarPDF")]
        public async Task<IActionResult> ExportarPrestamosAPdf()

        {
            // 1. Obtener la lista de préstamos desde el repositorio
            var listaPrestamos = await _prestamoRepositorio.Lista();

            // 2. Crear un stream de memoria para generar el PDF en memoria
            using (MemoryStream ms = new MemoryStream())
            {
                // Crear un documento PDF con márgenes ajustados
                Document doc = new Document(PageSize.Letter, 15, 15, 30, 30);
                PdfWriter writer = PdfWriter.GetInstance(doc, ms);

                // Abrir el documento para agregar contenido
                doc.Open();

                // Agregar imagen/logo superior
                string imageUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSUUTi3rRi4eZ7i171QcbRo-RkKiwC4_l9Ceg&s";
                try
                {
                    Image img = Image.GetInstance(new Uri(imageUrl));
                    img.ScaleToFit(100, 100); // Ajustar tamaño de la imagen
                    img.Alignment = Image.ALIGN_CENTER; // Alinear a la izquierda
                    doc.Add(img);
                }
                catch (Exception ex)
                {
                    // Manejar errores si no se puede cargar la imagen
                    Paragraph errorParagraph = new Paragraph("No se pudo cargar la imagen: " + ex.Message);
                    errorParagraph.Alignment = Element.ALIGN_CENTER;
                    doc.Add(errorParagraph);
                }

                // Información del estudiante y detalles dinámicos (asumiendo que vienen en el primer préstamo)
                var primerPrestamo = listaPrestamos.FirstOrDefault();
                if (primerPrestamo != null)
                {
                    PdfPTable infoTable = new PdfPTable(2); // Tabla con dos columnas
                    infoTable.WidthPercentage = 100;
                    infoTable.SetWidths(new float[] { 1, 2 }); // Ajustar ancho de columnas

                    PdfPCell cell1 = new PdfPCell(new Phrase("Teléfono:"));
                    PdfPCell cell2 = new PdfPCell(new Phrase("+505 8172 3835")); // Teléfono desde la fuente de datos
                    PdfPCell cell3 = new PdfPCell(new Phrase("Dirección:"));
                    PdfPCell cell4 = new PdfPCell(new Phrase("De donde fue Hulesa 1c. al Oeste, 1 1/2 c. al Sur. Colonia Villa Esperanza. Jinotepe - Carazo.")); // Dirección desde la fuente de datos
                    PdfPCell cell5 = new PdfPCell(new Phrase("Correo:"));
                    PdfPCell cell6 = new PdfPCell(new Phrase("manuelhernandez@gmail.com")); // Correo desde la fuente de datos

                    // Alineación y estilos
                    cell1.Border = PdfPCell.NO_BORDER;
                    cell2.Border = PdfPCell.NO_BORDER;
                    cell3.Border = PdfPCell.NO_BORDER;
                    cell4.Border = PdfPCell.NO_BORDER;
                    cell5.Border = PdfPCell.NO_BORDER;
                    cell6.Border = PdfPCell.NO_BORDER;

                    infoTable.AddCell(cell1);
                    infoTable.AddCell(cell2);
                    infoTable.AddCell(cell3);
                    infoTable.AddCell(cell4);
                    infoTable.AddCell(cell5);
                    infoTable.AddCell(cell6);

                    doc.Add(infoTable);
                }
                doc.Add(new Paragraph(" ")); // Espacio en blanco

                // Título
                Font tituloFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18, BaseColor.Black);
                Paragraph titulo = new Paragraph("Detalle de Préstamos", tituloFont)
                {
                    Alignment = Element.ALIGN_CENTER
                };
                doc.Add(titulo);
                doc.Add(new Paragraph(" ")); // Espacio en blanco

                // Crear la tabla con 7 columnas
                PdfPTable table = new PdfPTable(7);
                table.WidthPercentage = 100;

                // Encabezados
                Font cabeceraFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.White);
                BaseColor cabeceraFondo = new BaseColor(0, 0, 0);

                string[] cabeceras = { "ID Préstamo", "Código", "ID Lector", "ID Libro", "Fecha Devolución", "Estado Entregado", "Estado Recibido" };

                foreach (var cabecera in cabeceras)
                {
                    PdfPCell celdaCabecera = new PdfPCell(new Phrase(cabecera, cabeceraFont))
                    {
                        BackgroundColor = cabeceraFondo,
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        Padding = 5
                    };
                    table.AddCell(celdaCabecera);
                }

                // Añadir los datos de cada préstamo dinámicamente
                foreach (var prestamo in listaPrestamos)
                {
                    table.AddCell(new PdfPCell(new Phrase(prestamo.IdPrestamo.ToString())) { Padding = 5 });
                    table.AddCell(new PdfPCell(new Phrase(prestamo.Codigo)) { Padding = 5 });
                    table.AddCell(new PdfPCell(new Phrase(prestamo.IdLector.ToString())) { Padding = 5 });
                    table.AddCell(new PdfPCell(new Phrase(prestamo.IdLibro.ToString())) { Padding = 5 });
                    table.AddCell(new PdfPCell(new Phrase(prestamo.FechaDevolucion?.ToString("dd/MM/yyyy"))) { Padding = 5 });
                    table.AddCell(new PdfPCell(new Phrase(prestamo.EstadoEntregado)) { Padding = 5 });
                    table.AddCell(new PdfPCell(new Phrase(prestamo.EstadoRecibido)) { Padding = 5 });
                }

                // Añadir la tabla al documento
                doc.Add(table);

                // Cerrar el documento
                doc.Close();

                // Convertir a bytes el contenido del PDF
                byte[] pdfBytes = ms.ToArray();

                // Retornar el archivo PDF
                return File(pdfBytes, "application/pdf", "DetallePrestamos_" + DateTime.Now.ToString("yyyyMMdd") + ".pdf");
            }
        }




    }
}
