using WebServicesEnrollment.Models;
using System.Data.SqlClient;
using System.Data;
using WebServicesEnrollment.Helpers;

namespace WebServicesEnrollment.Services
{
    public class EnrollmentService : IEnrollmentService
    {
        private IConfiguration Configuration;
        private SqlConnection Connection = null;
 
        private AppLog AppLog = new AppLog();
        public EnrollmentService(IConfiguration Configuration)
        {
            this.Configuration = Configuration;
            this.Connection = new SqlConnection(this.Configuration.GetConnectionString("defaultConnection"));
        }
        public EnrollmentResponse EnrollmentProcess(EnrollmentRequest request)
        {
            AppLog.ResponseTime = Convert.ToInt16(DateTime.Now.ToString("fff"));
            AppLog.DateTime = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
            EnrollmentResponse respuesta = null;
            Aspirante aspirante = buscarAspirante(request.NoExpediente);
            if(aspirante == null)
            {
                respuesta = new EnrollmentResponse() {Codigo = 204, Respuesta =  "No existen registros"};
                Utils.ImprimirLog(204, $"No existen registros para el número de expediente {request.NoExpediente}","Information", this.AppLog);
            }
            else 
            {
                respuesta = EjecutarProcedimiento(request);
            }
            return respuesta;
        }

        private EnrollmentResponse EjecutarProcedimiento(EnrollmentRequest request)
        {
            EnrollmentResponse response = null;
            SqlCommand cmd = new SqlCommand("sp_EnrollmentProcess", this.Connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add(new SqlParameter("@NoExpediente", request.NoExpediente));
            cmd.Parameters.Add(new SqlParameter("@Ciclo", request.Ciclo));
            cmd.Parameters.Add(new SqlParameter("@MesInicioPago", request.MesInicioPago));
            cmd.Parameters.Add(new SqlParameter("@CarreraId", request.CarreraId));
            SqlDataReader reader = null;
            try
            {
                this.Connection.Open();
                reader = cmd.ExecuteReader();
                while(reader.Read())
                {
                    response = new EnrollmentResponse()
                        { 
                            Respuesta = reader.GetValue(0).ToString(), 
                            Carne = reader.GetValue(1).ToString()
                        };
                    if(reader.GetValue(0).ToString().Equals("TRANSACTION SUCCESS"))
                    {
                        response.Codigo = 201;
                        Utils.ImprimirLog(201,reader.GetValue(0).ToString(),"Information",this.AppLog);
                    }
                    else if(reader.GetValue(0).ToString().Equals("TRANSACTION ERROR"))
                    {
                        response.Codigo = 503;
                        Utils.ImprimirLog(503,reader.GetValue(0).ToString(),"Error",this.AppLog);
                    }
                    else 
                    {
                        response.Codigo = 503;
                        Utils.ImprimirLog(503,"Error al momento de llamar al procedimiento almacenado","Error",this.AppLog);
                    }                    
                }
                reader.Close();
                this.Connection.Close();
            }
            catch(Exception e)
            {
                response = new EnrollmentResponse(){Codigo = 503, Respuesta = "Error al momento de ejecutar el proceso de Inscripción", Carne = "0"};
                Utils.ImprimirLog(503,e.Message, "Error",this.AppLog);
            }
            finally
            {
                this.Connection.Close();
            }
            return response;
        }
        private Aspirante buscarAspirante(string noExpediente)
        {            
            Aspirante resultado = null;
            SqlDataAdapter adapter = new SqlDataAdapter($"select * from Aspirante a where a.NoExpediente  = '{noExpediente}'", this.Connection);
            DataSet dsAspirante = new DataSet();
            adapter.Fill(dsAspirante,"aspirante");
            if(dsAspirante.Tables["aspirante"].Rows.Count > 0)
            {
                resultado = new Aspirante() 
                {
                    NoExpediente = dsAspirante.Tables["aspirante"].Rows[0][0].ToString(), 
                    Apellidos = dsAspirante.Tables["aspirante"].Rows[0][1].ToString(), 
                    Nombres = dsAspirante.Tables["aspirante"].Rows[0][2].ToString(), 
                    Direccion = dsAspirante.Tables["aspirante"].Rows[0][3].ToString(), 
                    Telefono =  dsAspirante.Tables["aspirante"].Rows[0][4].ToString(), 
                    Email = dsAspirante.Tables["aspirante"].Rows[0][5].ToString(),
                    Estatus = dsAspirante.Tables["aspirante"].Rows[0][6].ToString(),
                    CarreraId = dsAspirante.Tables["aspirante"].Rows[0][7].ToString()
                };
            }
            return resultado;
        }

        public string Test(string s)
        {
            Console.WriteLine("Test method executed");
            return s;
        }
        
        public CandidateRecordResponse CandidateRegisterProcess(CandidateRecordRequest request)
        {
            AppLog.ResponseTime = Convert.ToInt16(DateTime.Now.ToString("fff"));
            AppLog.DateTime = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
            CandidateRecordResponse response = null;
            SqlCommand cmd = new SqlCommand("sp_CandidateRecordCreate", this.Connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add(new SqlParameter("@apellidos", request.Apellidos));
            cmd.Parameters.Add(new SqlParameter("@nombres", request.Nombres));
            cmd.Parameters.Add(new SqlParameter("@direccion", request.Direccion));
            cmd.Parameters.Add(new SqlParameter("@telefono", request.Telefono));
            cmd.Parameters.Add(new SqlParameter("@email", request.Email));
            cmd.Parameters.Add(new SqlParameter("@carreraId", request.CarreraId));
            cmd.Parameters.Add(new SqlParameter("@examenId", request.ExamenId));
            cmd.Parameters.Add(new SqlParameter("@jornadaId", request.JornadaId));
            SqlDataReader reader = null;
            try {
                this.Connection.Open();
                reader = cmd.ExecuteReader();
                while(reader.Read()) 
                {
                    response = new CandidateRecordResponse() 
                    { 
                        Respuesta = reader.GetValue(0).ToString(), 
                        NoExpediente = reader.GetValue(1).ToString()
                    };
                    if(reader.GetValue(0).ToString().Equals("TRANSACTION SUCCESS"))
                    {
                        response.Codigo = 201;
                        Utils.ImprimirLog(201,reader.GetValue(0).ToString(),"Information",this.AppLog);
                    }
                    else if(reader.GetValue(0).ToString().Equals("TRANSACTION ERROR"))
                    {
                        response.Codigo = 503;
                        Utils.ImprimirLog(503,reader.GetValue(0).ToString(),"Information",this.AppLog);

                    } else 
                    {
                        response.Codigo = 500;
                        Utils.ImprimirLog(500,"Error en el proceso para generar el expediente","Information",this.AppLog);
                    }
                }
                reader.Close();
                this.Connection.Close();
            }
            catch(Exception e) 
            {
                response = new CandidateRecordResponse(){Codigo = 503, Respuesta = "Error al momento de ejecutar el proceso de registro", NoExpediente = "0"};                
                Utils.ImprimirLog(500,e.Message,"Information",this.AppLog);

            }
            finally
            {
                this.Connection.Close();
            }
            return response;
        }
    }
}