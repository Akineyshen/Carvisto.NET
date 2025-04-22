document.addEventListener('DOMContentLoaded', function() {
    const swapBtn = document.querySelector('.swap-btn');

    swapBtn.addEventListener('click', function() {
        const startLocation = document.querySelector('input[name="StartLocation"]');
        const endLocation = document.querySelector('input[name="EndLocation"]');
        
        if (!startLocation || !endLocation) {
            console.error('No input fields found');
            return;
        }
        
        const temp = startLocation.value;
        startLocation.value = endLocation.value;
        endLocation.value = temp;
        
        this.style.transform = 'rotate(180deg)';
        setTimeout(() => {
            this.style.transform = 'rotate(0)';
        }, 300);
    });
});