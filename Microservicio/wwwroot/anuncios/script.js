document.addEventListener('DOMContentLoaded', () => {
    // Elementos del DOM
    const crearAnuncioSection = document.getElementById('crear-anuncio-section');
    const verAnunciosSection = document.getElementById('ver-anuncios-section');
    const anunciosLista = document.getElementById('anuncios-lista');
    const anuncioForm = document.getElementById('anuncioForm');

    // Enlaces del menú
    const crearLink = document.getElementById('crear-link');
    const verLink = document.getElementById('ver-link');

    // Muestra la sección para crear anuncio
    crearLink.addEventListener('click', (e) => {
        e.preventDefault();
        crearAnuncioSection.style.display = 'block';
        verAnunciosSection.style.display = 'none';
    });

    // Muestra la sección para ver los anuncios
    verLink.addEventListener('click', (e) => {
        e.preventDefault();
        crearAnuncioSection.style.display = 'none';
        verAnunciosSection.style.display = 'block';
    });

    // Agregar anuncio dinámicamente
    anuncioForm.addEventListener('submit', (e) => {
        e.preventDefault();

        // Obtener valores del formulario
        const titulo = document.getElementById('titulo').value;
        const contenido = document.getElementById('contenido').value;
        const urgente = document.getElementById('urgente').checked;
        const destacado = document.getElementById('destacado').checked;

        // Crear un nuevo div para el anuncio
        const anuncioDiv = document.createElement('div');
        anuncioDiv.classList.add('anuncio');

        // Agregar título y contenido
        anuncioDiv.innerHTML = `
            <h3>${titulo}</h3>
            <p>${contenido}</p>
        `;

        // Marcar como urgente o destacado si corresponde
        if (urgente) {
            const urgenteSpan = document.createElement('span');
            urgenteSpan.classList.add('urgente');
            urgenteSpan.textContent = 'Urgente';
            anuncioDiv.appendChild(urgenteSpan);
        }

        if (destacado) {
            const destacadoSpan = document.createElement('span');
            destacadoSpan.classList.add('destacado');
            destacadoSpan.textContent = 'Destacado';
            anuncioDiv.appendChild(destacadoSpan);
        }

        // Agregar el anuncio a la lista de anuncios
        anunciosLista.appendChild(anuncioDiv);

        // Resetear el formulario después de enviar
        anuncioForm.reset();

        // Cambiar automáticamente a la sección de ver anuncios
        crearAnuncioSection.style.display = 'none';
        verAnunciosSection.style.display = 'block';
    });
});
