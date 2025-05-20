/**
 * PowerOFF Landing Page - Script JS
 * Intégration de la nouvelle interface PowerOFF
 */

// Fonction pour initialiser le canvas de particules


// Fonction pour animer les compteurs de statistiques
function initCounters() {
    const counters = document.querySelectorAll('.stat-number');
    
    const options = {
        root: null,
        rootMargin: '0px',
        threshold: 0.1
    };

    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                const counter = entry.target;
                const target = parseFloat(counter.getAttribute('data-count'));
                const duration = 2000; // ms
                const step = target / (duration / 16); // 60fps
                let current = 0;

                const updateCounter = () => {
                    if (current < target) {
                        current += step;
                        if (current > target) current = target;
                        
                        // Formater le nombre selon qu'il s'agit d'un entier ou d'un décimal
                        if (Number.isInteger(target)) {
                            counter.textContent = Math.floor(current).toLocaleString();
                        } else {
                            counter.textContent = current.toFixed(1);
                        }
                        
                        requestAnimationFrame(updateCounter);
                    }
                };

                updateCounter();
                observer.unobserve(counter);
            }
        });
    }, options);

    counters.forEach(counter => {
        observer.observe(counter);
    });
}

// Fonction pour animer les étapes de la démo
function initDemoSteps() {
    const steps = document.querySelectorAll('.demo-step');
    if (steps.length === 0) return;

    let currentStep = 0;

    function showStep(index) {
        steps.forEach((step, i) => {
            step.classList.toggle('active', i === index);
        });
    }

    // Changer d'étape toutes les 3 secondes
    setInterval(() => {
        currentStep = (currentStep + 1) % steps.length;
        showStep(currentStep);
    }, 3000);
}

// Fonction pour mettre à jour l'heure et la date actuelles
function updateDateTime() {
    const timeDisplay = document.querySelector('.time');
    const dateDisplay = document.querySelector('.date');
    
    if (!timeDisplay || !dateDisplay) return;

    function update() {
        const now = new Date();
        
        // Format de l'heure: HH:MM
        const hours = String(now.getHours()).padStart(2, '0');
        const minutes = String(now.getMinutes()).padStart(2, '0');
        const seconds = String(now.getSeconds()).padStart(2, '0');
        timeDisplay.textContent = `${hours}:${minutes}:${seconds}`;
        
        // Format de la date: JJ/MM/YYYY
        const day = String(now.getDate()).padStart(2, '0');
        const month = String(now.getMonth() + 1).padStart(2, '0');
        const year = now.getFullYear();
        dateDisplay.textContent = `${day}/${month}/${year}`;
    }

    // Mettre à jour immédiatement puis toutes les minutes
    update();
    setInterval(update, 1000);
}


// Initialisation au chargement de la page
document.addEventListener('DOMContentLoaded', () => {
    initCounters();
    initDemoSteps();
    updateDateTime();
});

// Gestion du défilement fluide pour les liens d'ancrage
document.querySelectorAll('a[href^="#"]').forEach(anchor => {
    anchor.addEventListener('click', function (e) {
        e.preventDefault();
        
        const targetId = this.getAttribute('href');
        if (targetId === '#') return;
        
        const targetElement = document.querySelector(targetId);
        if (!targetElement) return;
        
        window.scrollTo({
            top: targetElement.offsetTop,
            behavior: 'smooth'
        });
    });
});


