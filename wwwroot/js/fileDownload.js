// Función para descargar archivos desde Blazor
window.downloadFile = function (fileName, base64String, contentType) {
    // Convertir base64 a blob
    const byteCharacters = atob(base64String);
    const byteNumbers = new Array(byteCharacters.length);
    for (let i = 0; i < byteCharacters.length; i++) {
        byteNumbers[i] = byteCharacters.charCodeAt(i);
    }
    const byteArray = new Uint8Array(byteNumbers);
    const blob = new Blob([byteArray], { type: contentType });

    // Crear enlace temporal y hacer clic para descargar
    const link = document.createElement('a');
    link.href = window.URL.createObjectURL(blob);
    link.download = fileName;
    link.click();

    // Limpiar
    window.URL.revokeObjectURL(link.href);
};