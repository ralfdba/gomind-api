namespace gomind_backend_api.Models.Errors
{
    public static class CommonErrors
    {
        #region Token
        public static readonly string MissingToken = "Ingrese todos los parámetros necesarios (Token).";
        public static readonly string InvalidToken = "Token no válido o expirado.";
        public static readonly string UserTokenNoValid1 = "Error al intentar obtener el usuario.";
        #endregion

        #region Login
        public static readonly string MissingCredentials = "Debe ingresar el correo y password.";
        public static readonly string MissingCredentials2 = "Debe ingresar el correo.";
        public static readonly string MissingCredentials3 = "Debe ingresar el correo y código de verificación.";
        public static readonly string InvalidCredentials = "No se ha podido iniciar sesión.";
        public static readonly string UserNotFound = "No encontramos una cuenta asociada a este correo electrónico. Por favor, verifica que esté bien escrito o regístrate para crear una cuenta nueva.";
        #endregion

        #region Generic Errors
        public static string UnexpectedError(string details) => $"Error inesperado: {details}";
        public static string ValidationFailed = "Validación fallida. Por favor, revise los campos.";
        public static readonly string GenericNoValid1 = "El dato ingresado no es válido.";
        public static readonly string GenericNoValid2 = "No se pudo grabar la información, intenta nuevamente.";
        public static readonly string GenericNoValid3 = "No existe un registro con el id proporcionado.";
        public static readonly string BadRequest1 = "Ingrese todos los parámetros necesarios.";
        public static readonly string UserIdNoValid = "El usuario no es válido.";
        public static readonly string ProductIdNoValid = "El producto no es válido.";
        #endregion

        #region File
        public static readonly string FileNoUploaded = "No se ha subido ningún archivo.";
        public static readonly string FileTypeNoValidPDF = "Solo se pueden subir archivos PDF.";
        #endregion

        #region Job
        public static readonly string JobNotFound = "No se encontro ningun job con el proporcionado.";
        public static readonly string JobNotValid = "El job ingresado debe estar Completed y sin errores.";
        public static readonly string JobKeyResultNull = "El key result no puede ser null.";
        #endregion

        #region Reference Range
        public static readonly string ReferenceRangeNoValid1 = "Debe especificar un valor mínimo y máximo para la referencia de rango.";
        public static readonly string ReferenceRangeNoValid2 = "Debe especificar un valor el campo 'condition_value'.";
        public static readonly string ReferenceRangeNoValid3 = "El rango mínimo debe ser inferior al rango máximo.";

        #endregion


        #region ValidationError
        public static MessageResponse ValidationError(Dictionary<string, string[]> errors)
        {
            return MessageResponse.Create(ValidationFailed, errors);
        }
        #endregion
    }
}
