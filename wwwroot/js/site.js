// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.
// Write your JavaScript code.
document.addEventListener('DOMContentLoaded', function() {
    const swapBtn = document.querySelector('.swap-btn');

    swapBtn.addEventListener('click', function() {
        const startLocation = document.querySelector('input[name="StartLocation"]');
        const endLocation = document.querySelector('input[name="EndLocation"]');

        // Проверка на существование элементов
        if (!startLocation || !endLocation) {
            console.error('Не найдены поля ввода');
            return;
        }

        // Меняем значения местами
        const temp = startLocation.value;
        startLocation.value = endLocation.value;
        endLocation.value = temp;

        // Добавим анимацию для визуального подтверждения
        this.style.transform = 'rotate(180deg)';
        setTimeout(() => {
            this.style.transform = 'rotate(0)';
        }, 300);
    });
});

// Header