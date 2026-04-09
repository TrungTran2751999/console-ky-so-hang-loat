using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VnptHashSignatures.Common;
using VnptHashSignatures.Interface;
using VnptHashSignatures.Pdf;
using System.Threading;
using VnptHashSignatures.Xml;
using static System.Net.WebRequestMethods;
using File = System.IO.File;
using RestSharp;
using System.Security.Cryptography.X509Certificates;
using Newtonsoft.Json;
using System.IO;
using itextSharpPDFText = iTextSharp.text;
using itextSharpPDF = iTextSharp.text.pdf;
using iTextSharp.text.pdf;
using iTextSharp.text;
using itext7 = iText.Kernel.Pdf;

namespace ConsoleKySo
{
    public class Program
    {
        public static string pathChuKy = "C:\\Users\\COMPUTER\\Desktop\\test-pdf\\ChuKy.png";
        public static string pathDoc = "C:\\Users\\COMPUTER\\Desktop\\test-pdf\\SAMPLE.pdf";
        public static string pathOutput = "C:\\Users\\COMPUTER\\Desktop\\test-pdf\\sign";
        static void Main(string[] args)
        {
            var program = new Program();
            program.Test2();
        }
        public void Test2()
        {
            var loginEntity = new LoginEntity
            {
                UserName = "046099012704",
                Password = "Trung1708040517."
            };
            var resultLogin = LoginVnptCA(loginEntity);

            var program = new Program();
            //ghep file pdf
            var listBase = new List<string>();
            for (var i = 1; i <= 1; i++)
            {
                var base64 = Convert.ToBase64String(File.ReadAllBytes(pathDoc));
                listBase.Add(base64);
            }
            var fileGhepPdf = program.GhepBase64ToPDF(listBase);
            //dinh vi moi trang mot chu ky
            var docsig = new doc_sign
            {
                img = program.ConvertBase644(pathChuKy),
                //signatures = listSignatures,
                access_token = resultLogin.token,
                base64Doc = fileGhepPdf,
                tenVB = "test"
            };
            program.SignSeries(docsig);
        }
        public void Test1()
        {
            var loginEntity = new LoginEntity
            {
                UserName = "046099012704",
                Password = "Trung1708040517."
                //UserName = "046060004338",
                //Password = "Bom753159."
            };
            var resultLogin = LoginVnptCA(loginEntity);

            var program = new Program();
            //ghep file pdf
            var listBase = new List<string>();
            for (var i = 1; i <= 1; i++)
            {
                var base64 = Convert.ToBase64String(File.ReadAllBytes(pathDoc));
                listBase.Add(base64);
            }
            var fileGhepPdf = program.GhepBase64ToPDF(listBase);
            //dinh vi moi trang mot chu ky
            var listSignatures = new List<SignatureItem>();
            for (var i = 1; i <= 1; i++)
            {
                listSignatures.Add(new SignatureItem
                {
                    rectangle = "100,100,300,200",
                    page = i
                });
            }
            var docsig = new doc_sign
            {
                img = program.ConvertBase644(pathChuKy),
                signatures = listSignatures,
                access_token = resultLogin.token,
                base64Doc = fileGhepPdf,
                tenVB = "test"
            };
            var byteList = Convert.FromBase64String(program.Sign(docsig));
            program.TachTrangPdf(byteList, pathOutput);
            File.WriteAllBytes("C:\\Users\\COMPUTER\\Desktop\\test-pdf\\sign1.pdf", byteList);
        }
        public string GetAccessToken(LoginEntity _request)
        {
            string refresh_token = "";
            var result = new AccessToken();
            result.token = CoreServiceClient.GetAccessToken(_request.UserName, _request.Password, out refresh_token);
            result.refresh_token = refresh_token;

            //return Request.CreateResponse<AccessToken>(HttpStatusCode.OK, result);
            return result.refresh_token;
        }
        public void TachTrangPdf(byte[] bytePdf, string outputDirectory)
        {
            //FileInfo file = new FileInfo(inputPath);
            //string name = file.Name.Substring(0, file.Name.LastIndexOf("."));

            using (PdfReader reader = new PdfReader(bytePdf))
            {

                for (int pagenumber = 1; pagenumber <= reader.NumberOfPages; pagenumber++)
                {
                    string filename = pagenumber.ToString() + ".pdf";

                    Document document = new Document();
                    PdfCopy copy = new PdfCopy(document, new FileStream(outputDirectory + "\\" + filename, FileMode.Create));

                    document.Open();

                    copy.AddPage(copy.GetImportedPage(reader, pagenumber));

                    document.Close();
                }
            }
        }
        public void TachTrangPdf1(string outputDirectory)
        {
            string sourcePdfPath = "path/to/source.pdf";

            Directory.CreateDirectory(outputDirectory);

            using (itext7.PdfReader pdfReader = new itext7.PdfReader(sourcePdfPath))
            using (itext7.PdfDocument pdfDoc = new itext7.PdfDocument(pdfReader))
            {
                int pageCount = pdfDoc.GetNumberOfPages();

                for (int i = 1; i <= pageCount; i++)
                {
                    string outputPdfPath = Path.Combine(outputDirectory, $"page_{i}.pdf");
                    using (itext7.PdfWriter pdfWriter = new itext7.PdfWriter(outputPdfPath))
                    using (itext7.PdfDocument newPdfDoc = new itext7.PdfDocument(pdfWriter))
                    {
                        // Chỉ sao chép trang mà không làm thay đổi nội dung
                        pdfDoc.CopyPagesTo(i, i, newPdfDoc);
                    }
                }
            }

            Console.WriteLine("Tách từng trang thành công mà không mất chữ ký số!");
        }
        //public HttpResponseMessage LoginVnptCA(LoginEntity _request)
        //{
        //    string str = "";
        //    string accessToken = CoreServiceClient.GetAccessToken(_request.UserName, _request.Password, out str);
        //    HttpResponseMessage httpResponseMessage = Request.CreateResponse(HttpStatusCode.OK, new { token = accessToken, refresh_token = str });
        //    return httpResponseMessage;
        //}
        public static ResultLoginVnptCA LoginVnptCA(LoginEntity _request)
        {
            string str = "";
            string accessToken = CoreServiceClient.GetAccessToken(_request.UserName, _request.Password, out str);
            //HttpResponseMessage httpResponseMessage = Request.CreateResponse(HttpStatusCode.OK, new { token = accessToken, refresh_token = str });
            var result = new ResultLoginVnptCA { token = accessToken, refresh_token = str };
            return result;
        }
        public string ConvertBase644(string pathfile)
        {
            var result = File.ReadAllBytes(pathfile);
            return Convert.ToBase64String(result);
        }
        private string GhepBase64ToPDF(List<string> listBase64)
        {
            if (listBase64.Count > 0)
            {
                using (var outputPdfStream = new MemoryStream())
                {
                    var pdfDocument = new iTextSharp.text.Document();

                    var pdfCopy = new itextSharpPDF.PdfCopy(pdfDocument, outputPdfStream);

                    pdfDocument.Open();

                    foreach (var base64Str in listBase64)
                    {
                        byte[] pdfBytes = Convert.FromBase64String(base64Str);

                        var inputPdf = new itextSharpPDF.PdfReader(pdfBytes);
                        int pageCount = inputPdf.NumberOfPages;
                        for (int i = 1; i <= pageCount; i++)
                        {
                            pdfCopy.AddPage(pdfCopy.GetImportedPage(inputPdf, i));
                        }
                    }

                    pdfDocument.Close();

                    byte[] mergedPdfBytes = outputPdfStream.ToArray();
                    return Convert.ToBase64String(mergedPdfBytes);
                }
            }
            else
            {
                return JsonConvert.SerializeObject(listBase64);
            }

        }
        public string Sign(doc_sign doc)
        {
            string credentialSmartCa = this._getCredentialSmartCA(doc.access_token, "https://gwsca.vnpt.vn/csc/credentials/list");
            string accoutSmartCaCert = this._getAccoutSmartCACert(doc.access_token, "https://gwsca.vnpt.vn/csc/credentials/info", credentialSmartCa);

            byte[] numArray1 = Convert.FromBase64String(doc.base64Doc);

            IHashSigner signer = HashSignerFactory.GenerateSigner(numArray1, accoutSmartCaCert, (string)null, "PDF");
            ((PdfHashSigner)signer).SetHashAlgorithm((MessageDigestAlgorithm)1);
            ((PdfHashSigner)signer).SetReason("Xác nhận tài liệu");
            byte[] numArray2 = Convert.FromBase64String(doc.img);
            ((PdfHashSigner)signer).SetCustomImage(numArray2);
            ((PdfHashSigner)signer).SetRenderingMode((PdfHashSigner.RenderMode)3);
            foreach (SignatureItem signature in doc.signatures)
                ((PdfHashSigner)signer).AddSignatureView(new PdfSignatureView()
                {
                    Rectangle = signature.rectangle,
                    Page = signature.page
                });
            ((PdfHashSigner)signer).SetSignatureBorderType((PdfHashSigner.VisibleSigBorder)1);
            string secondHashAsBase64 = signer.GetSecondHashAsBase64();
            string tranId = _signHash(doc.access_token, "https://gwsca.vnpt.vn/csc/signature/signhash", secondHashAsBase64, credentialSmartCa, doc.tenVB);
            if (tranId == "")
                return "ERROR";
            int num2 = 0;
            bool flag2 = false;
            string str3 = "";
            while (num2 < 24 && !flag2)
            {
                TranInfoSmartCARespContent tranInfo = _getTranInfo(doc.access_token, "https://gwsca.vnpt.vn/csc/credentials/gettraninfo", tranId);
                if (tranInfo == null)
                    return "ERROR";
                if (tranInfo.tranStatus != 1)
                {
                    ++num2;
                    Thread.Sleep(10000);
                }
                else
                {
                    flag2 = true;
                    str3 = tranInfo.documents[0].sig;
                }
            }
            if (!flag2 || string.IsNullOrEmpty(str3) || !signer.CheckHashSignature(str3))
                return "ERROR";

            byte[] bytes = signer.Sign(str3);
            return Convert.ToBase64String(bytes);
        }
        public string SignSeries(doc_sign doc)
        {
            //B1: Ky so
            string credentialSmartCa = this._getCredentialSmartCA(doc.access_token, "https://gwsca.vnpt.vn/csc/credentials/list");
            string accoutSmartCaCert = this._getAccoutSmartCACert(doc.access_token, "https://gwsca.vnpt.vn/csc/credentials/info", credentialSmartCa);

            List<byte[]> listUnsignData = new List<byte[]>();
            for(var i=1; i<=10; i++)
            {
                listUnsignData.Add(File.ReadAllBytes(pathDoc));
            }
            List<IHashSigner> signers = new List<IHashSigner>();
            List<KySoParam> data_to_be_signs = new List<KySoParam>();

            for(var i = 0; i<listUnsignData.Count; i++)
            {
               var unsignData = listUnsignData[i];
               var signer = HashSignerFactory.GenerateSigner(unsignData, accoutSmartCaCert, null, HashSignerFactory.PDF);
               ((PdfHashSigner)signer).SetHashAlgorithm((MessageDigestAlgorithm)1);
               ((PdfHashSigner)signer).SetReason("Xác nhận tài liệu");
               byte[] numArray2 = Convert.FromBase64String(doc.img);
               ((PdfHashSigner)signer).SetCustomImage(numArray2);
               ((PdfHashSigner)signer).SetRenderingMode((PdfHashSigner.RenderMode)3);

                //foreach (SignatureItem signature in doc.signatures)
                ((PdfHashSigner)signer).AddSignatureView(new PdfSignatureView()
                {
                    Rectangle = "100,100,300,200",//signature.rectangle,
                    Page = 1 //signature.page
                });
                ((PdfHashSigner)signer).SetSignatureBorderType((PdfHashSigner.VisibleSigBorder)1);

                string hashValue = signer.GetSecondHashAsBase64();

                var data_to_be_sign = BitConverter.ToString(Convert.FromBase64String(hashValue)).Replace("-", "").ToLower();
                data_to_be_signs.Add(new KySoParam
                {
                    Hash = data_to_be_sign,
                    TenVb = $"test-{i}"
                });
                signers.Add(signer);
            }
            var tranId = _signHashList(doc.access_token, "https://gwsca.vnpt.vn/csc/signature/signhash", data_to_be_signs, credentialSmartCa);
            //B2: Cho xac thuc
            if (tranId == "")
                return "ERROR";
            int num2 = 0;
            bool flag2 = false;
            string str3 = "";
            while (num2 < 30 && !flag2)
            {
                TranInfoSmartCARespContent tranInfo = _getTranInfo(doc.access_token, "https://gwsca.vnpt.vn/csc/credentials/gettraninfo", tranId);
                if (tranInfo == null)
                    return "ERROR";
                if (tranInfo.tranStatus != 1)
                {
                    ++num2;
                    Thread.Sleep(10000);
                }
                else
                {
                    flag2 = true;
                    str3 = tranInfo.documents[0].sig;
                }
            }
            if (!flag2 || string.IsNullOrEmpty(str3))
                return "ERROR";
            return "";

        }
        
        private string _getCredentialSmartCA(string accessToken, string serviceUri)
        {
            string str = CoreServiceClient.Query((object)new ReqCredentialSmartCA(), serviceUri, accessToken);
            return str != null ? JsonConvert.DeserializeObject<CredentialSmartCAResponse>(str).content[0] : "";
        }
        private string _getAccoutSmartCACert(string accessToken, string serviceUri, string credentialId)
        {
            string str = CoreServiceClient.Query((object)new ReqCertificateSmartCA()
            {
                credentialId = credentialId,
                certificates = "chain",
                certInfo = true,
                authInfo = true
            }, serviceUri, accessToken);
            return str != null ? JsonConvert.DeserializeObject<CertificateSmartCAResponse>(str).cert.certificates[0].Replace("\r\n", "") : "";
        }
        private static string _signHash(string accessToken, string serviceUri, string data, string credentialId, string tenVB)
        {
            SignHashSmartCAReq req = new SignHashSmartCAReq()
            {
                credentialId = credentialId,
                refTranId = new Guid().ToString(),
                notifyUrl = "http://10.169.0.221/api/v1/smart_ca_callback",
                description = "Ký số SmartCA",
                datas = new List<DataSignHash>()
            };
            DataSignHash dataSignHash = new DataSignHash()
            {
                name = tenVB + ".pdf",
                hash = data
            };
            req.datas.Add(dataSignHash);
            string str = CoreServiceClient.Query((object)req, serviceUri, accessToken);
            if (str != null)
            {
                SignHashSmartCAResponse hashSmartCaResponse = JsonConvert.DeserializeObject<SignHashSmartCAResponse>(str);
                if (hashSmartCaResponse.code == 0)
                    return hashSmartCaResponse.content.tranId;
            }
            return "";
        }
        private static string _signHashList(string accessToken, string serviceUri, List<KySoParam> listData, string credentialId)
        {
            SignHashSmartCAReq req = new SignHashSmartCAReq()
            {
                credentialId = credentialId,
                refTranId = new Guid().ToString(),
                notifyUrl = "http://10.169.0.221/api/v1/smart_ca_callback",
                description = "Ký số SmartCA",
                datas = new List<DataSignHash>()
            };
            foreach(var data in listData)
            {
                DataSignHash dataSignHash = new DataSignHash()
                {
                    name = data.TenVb + ".pdf",
                    hash = data.Hash
                };
                req.datas.Add(dataSignHash);
            }
            
            string str = CoreServiceClient.Query((object)req, serviceUri, accessToken);
            if (str != null)
            {
                SignHashSmartCAResponse hashSmartCaResponse = JsonConvert.DeserializeObject<SignHashSmartCAResponse>(str);
                if (hashSmartCaResponse.code == 0)
                    return hashSmartCaResponse.content.tranId;
            }
            return "";
        }
        private TranInfoSmartCARespContent _getTranInfo(string accessToken, string serviceUri, string tranId)
        {
            string str = CoreServiceClient.Query((object)new ContenSignHash()
            {
                tranId = tranId
            }, serviceUri, accessToken);
            if (str != null)
            {
                TranInfoSmartCAResp tranInfoSmartCaResp = JsonConvert.DeserializeObject<TranInfoSmartCAResp>(str);
                if (tranInfoSmartCaResp.code == 0)
                    return tranInfoSmartCaResp.content;
            }
            return (TranInfoSmartCARespContent)null;
        }
    }
    public class CoreServiceClient
    {
        private static readonly string SERVICE_GET_TOKENURL = "https://gwsca.vnpt.vn/auth/token";
        private static readonly string APP_ID = "49b5-637745563154765100.apps.smartcaapi.com";
        private static readonly string APP_SECRET = "MDZhOGJmN2Y-MmE2ZC00OWI1";
        public static string GetAccessToken(string email, string pass, out string refresh_token)
        {
            refresh_token = "";
            ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(SslHelper.ValidateRemoteCertificate);
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            RestClient restClient = new RestClient(SERVICE_GET_TOKENURL);
            RestRequest restRequest = new RestRequest(Method.POST);
            CoreServiceClient.GetTokenBody getTokenBody = new CoreServiceClient.GetTokenBody()
            {
                grant_type = "password",
                username = email,
                password = pass,
                client_id = APP_ID,
                client_secret = APP_SECRET
            };
            JsonConvert.SerializeObject((object)getTokenBody);
            restRequest.AddHeader("content-type", "application/x-www-form-urlencoded");
            restRequest.AddParameter("application/x-www-form-urlencoded", (object)("grant_type=" + getTokenBody.grant_type + "&username=" + getTokenBody.username + "&password=" + getTokenBody.password + "&client_id=" + getTokenBody.client_id + "&client_secret=" + getTokenBody.client_secret), (ParameterType)4);
            IRestResponse irestResponse;
            try
            {
                irestResponse = restClient.Execute((IRestRequest)restRequest);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            if (irestResponse == null || irestResponse.ErrorException != null)
                return (string)null;
            Encoding.GetEncoding("ISO-8859-1").GetString(irestResponse.RawBytes);
            if (irestResponse.StatusCode != HttpStatusCode.OK)
                return (string)null;
            string content = irestResponse.Content;
            try
            {
                CoreServiceClient.GetTokenResponse getTokenResponse = JsonConvert.DeserializeObject<CoreServiceClient.GetTokenResponse>(content);
                refresh_token = getTokenResponse.refresh_token;
                return getTokenResponse.access_token;
            }
            catch (Exception ex)
            {
                return (string)null;
            }
        }

        public static string Query(object req, string serviceUri, string accessToken)
        {
            ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(SslHelper.ValidateRemoteCertificate);
            RestClient restClient = new RestClient(serviceUri);
            RestRequest restRequest = new RestRequest((Method)1)
            {
                RequestFormat = (DataFormat)0
            };
            restRequest.AddBody(req);
            restRequest.AddHeader("Authorization", "Bearer " + accessToken);
            IRestResponse irestResponse = (IRestResponse)null;
            try
            {
                irestResponse = restClient.Execute((IRestRequest)restRequest);
            }
            catch (Exception ex)
            {
            }
            return irestResponse == null || irestResponse.ErrorException != null || irestResponse.StatusCode != HttpStatusCode.OK ? (string)null : irestResponse.Content;
        }

        private class GetTokenBody
        {
            public string grant_type { get; set; }

            public string username { get; set; }

            public string password { get; set; }

            public string client_id { get; set; }

            public string client_secret { get; set; }
        }

        private class GetTokenResponse
        {
            public string access_token { get; set; }

            public string refresh_token { get; set; }

            public string token_type { get; set; }

            public int expires_in { get; set; }
        }

        public class ResponseMessage
        {
            public Guid ResponseID { get; set; }

            public int ResponseCode { get; set; }

            public string ResponseContent { get; set; }

            public object Content { get; set; }
        }

        public class RequestMessage
        {
            public string RequestID { get; set; }

            public string ServiceID { get; set; }

            public string FunctionName { get; set; }

            public object Parameter { get; set; }
        }
    }
    public class SslHelper
    {
        public static bool ValidateRemoteCertificate(object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors policyErrors)
        {
            bool result = true;
            return result;
        }
    }
    public class LoginEntity
    {
        public string UserName { get; set; }

        public string Password { get; set; }
    }
    public class ResultLoginVnptCA
    {
        public string token { get; set; }
        public string refresh_token { get; set; }
    }

    public class AccessToken
    {
        public string token { get; set; }

        public string refresh_token { get; set; }
    }

    public class SignatureItem
    {
        public string rectangle { get; set; }

        public int page { get; set; }
    }

    public class DataSignHash
    {
        public string name { get; set; }

        public string hash { get; set; }
    }

    public class ContenSignHash
    {
        public string tranId { get; set; }
    }

    public class SignHashSmartCAReq
    {
        public string credentialId { get; set; }

        public string refTranId { get; set; }

        public string notifyUrl { get; set; }

        public string description { get; set; }

        public List<DataSignHash> datas { get; set; }
    }

    public class SignHashSmartCAResponse
    {
        public int code { get; set; }

        public string codeDesc { get; set; }

        public string message { get; set; }

        public ContenSignHash content { get; set; }
    }

    public class CertificateSmartCAResponse
    {
        public CertRes cert { get; set; }

        public keyRes key { get; set; }

        public string authMode { get; set; }

        public string scal { get; set; }

        public string mutisign { get; set; }

        public string status { get; set; }
    }

    public class CredentialSmartCAResponse
    {
        public int code { get; set; }

        public string codeDesc { get; set; }

        public string message { get; set; }

        public List<string> content { get; set; }
    }

    public class CertRes
    {
        public string status { get; set; }

        public string serialNumber { get; set; }

        public string subjectDN { get; set; }

        public string issuerDN { get; set; }

        public List<string> certificates { get; set; }

        public string validFrom { get; set; }

        public string validTo { get; set; }
    }

    public class keyRes
    {
        public string status { get; set; }

        public List<string> alg { get; set; }

        public int len { get; set; }
    }

    public class ReqCredentialSmartCA
    {
    }

    public class ReqCertificateSmartCA
    {
        public string credentialId { get; set; }

        public string certificates { get; set; }

        public bool certInfo { get; set; }

        public bool authInfo { get; set; }
    }

    public class TranInfoSmartCARespContent
    {
        public string refTranId { get; set; }

        public string tranId { get; set; }

        public string sub { get; set; }

        public string credentialId { get; set; }

        public int tranType { get; set; }

        public string tranTypeDesc { get; set; }

        public int tranStatus { get; set; }

        public string transStatusDesc { get; set; }

        public DateTime? reqTime { get; set; }

        public List<DocumentResp> documents { get; set; }
    }

    public class DocumentResp
    {
        public string name { get; set; }

        public string type { get; set; }

        public string size { get; set; }

        public string data { get; set; }

        public string hash { get; set; }

        public string sig { get; set; }

        public string signature { get; set; }

        public string dataSigned { get; set; }

        public string url { get; set; }
    }

    public class TranInfoSmartCAResp
    {
        public int code { get; set; }

        public string codeDesc { get; set; }

        public string message { get; set; }

        public TranInfoSmartCARespContent content { get; set; }
    }

    public class doc_sign
    {
        public string filePath { get; set; }
        public string access_token { get; set; }
        public string refresh_token { get; set; }
        public string idNguoiThucHien { get; set; }
        public string idVB { get; set; }
        public string maFDK { get; set; }
        public string img { get; set; }
        public string daKySo { get; set; }
        public List<SignatureItem> signatures { get; set; }
        public string base64Doc { get; set; }
        public string tenVB { get; set; }
    }
    public class KySoParam
    {
        public string TenVb { get; set; }
        public string Hash { get; set; }
    }
}
