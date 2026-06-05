// module.js - Search and Filter functionality

function regenerateCode(moduleId) {
    if (!confirm('Generate a new invitation code? The previous code will be invalidated.')) {
        return;
    }

    const submitBtn = document.querySelector('.regenerate-btn');
    if (!submitBtn) return;
    
    const originalText = submitBtn.textContent;
    submitBtn.textContent = 'Generating...';
    submitBtn.disabled = true;

    fetch(`/api/module/${moduleId}/regenerate-code`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        }
    })
    .then(response => {
        if (!response.ok) {
            return response.text().then(text => { throw new Error(text || 'Server error'); });
        }
        return response.json();
    })
    .then(data => {
        if (data.success) {
            const codeElement = document.getElementById('invitationCode');
            if (codeElement) {
                codeElement.textContent = data.code;
                codeElement.style.backgroundColor = '#d4edda';
                codeElement.style.transition = '0.3s';
                setTimeout(() => {
                    codeElement.style.backgroundColor = '';
                }, 500);
            }
            alert('Invitation code regenerated successfully! New code: ' + data.code);
        } else {
            alert('Failed: ' + (data.message || 'Unknown error'));
        }
    })
    .catch(error => {
        console.error('Error:', error);
        alert('An error occurred: ' + error.message);
    })
    .finally(() => {
        submitBtn.textContent = originalText;
        submitBtn.disabled = false;
    });
}

// Search and Filter functionality
function filterModules() {
    console.log('filterModules called'); // Debug log
    
    const searchInput = document.getElementById('searchModule');
    const filterSelect = document.getElementById('filterModule');
    
    // If elements don't exist (student view), exit
    if (!searchInput || !filterSelect) {
        console.log('Search/filter elements not found (student view)');
        return;
    }
    
    const searchTerm = searchInput.value.toLowerCase().trim();
    const filterValue = filterSelect.value;
    
    console.log('Search term:', searchTerm, 'Filter:', filterValue); // Debug log
    
    const modules = document.querySelectorAll('.module-card');
    let visibleCount = 0;
    
    modules.forEach(module => {
        const title = module.querySelector('.module-title')?.textContent.toLowerCase() || '';
        const code = module.querySelector('.module-code')?.textContent.toLowerCase() || '';
        const description = module.querySelector('.module-description')?.textContent.toLowerCase() || '';
        
        // Check if module matches search term
        const matchesSearch = searchTerm === '' || 
                              title.includes(searchTerm) || 
                              code.includes(searchTerm) || 
                              description.includes(searchTerm);
        
        // TODO: Add filter logic for active/completed modules when you have status data
        // For now, filter value only affects "all" vs others
        let matchesFilter = true;
        if (filterValue === 'active') {
            // Add logic to check if module is active
            matchesFilter = true; // Placeholder
        } else if (filterValue === 'completed') {
            // Add logic to check if module is completed
            matchesFilter = true; // Placeholder
        }
        
        if (matchesSearch && matchesFilter) {
            module.classList.remove('hidden');
            visibleCount++;
        } else {
            module.classList.add('hidden');
        }
    });
    
    console.log('Visible modules:', visibleCount); // Debug log
    
    // Handle empty results
    const modulesContainer = document.querySelector('.PageComponent');
    const originalEmptyState = modulesContainer?.querySelector('.empty-state:not(.no-results-empty)');
    let noResultsMsg = modulesContainer?.querySelector('.no-results-empty');
    
    if (visibleCount === 0 && modulesContainer) {
        // Check if there's already a "no results" message
        if (!noResultsMsg) {
            const emptyDiv = document.createElement('div');
            emptyDiv.className = 'empty-state no-results-empty';
            emptyDiv.innerHTML = `
                <span><i class="bi bi-search"></i></span>
                <h3>No matching modules found</h3>
                <p>Try adjusting your search or filter criteria</p>
            `;
            modulesContainer.appendChild(emptyDiv);
        }
        // Hide original empty state if it exists and is visible
        if (originalEmptyState) {
            originalEmptyState.style.display = 'none';
        }
    } else {
        // Remove no-results message if it exists
        if (noResultsMsg) {
            noResultsMsg.remove();
        }
        // Show original empty state if it exists and no modules
        if (originalEmptyState && visibleCount === 0) {
            originalEmptyState.style.display = '';
        } else if (originalEmptyState) {
            originalEmptyState.style.display = 'none';
        }
    }
}

// Initialize event listeners - Run when DOM is ready
(function init() {
    // Wait for DOM to be fully loaded
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', function() {
            console.log('DOM loaded - initializing module page');
            setupEventListeners();
        });
    } else {
        console.log('DOM already loaded - initializing module page');
        setupEventListeners();
    }
})();

function setupEventListeners() {
    const searchInput = document.getElementById('searchModule');
    const filterSelect = document.getElementById('filterModule');
    
    if (searchInput) {
        console.log('Search input found, attaching event listener');
        searchInput.addEventListener('input', function() {
            console.log('Search input changed:', this.value);
            filterModules();
        });
    } else {
        console.log('Search input not found (student view)');
    }
    
    if (filterSelect) {
        console.log('Filter select found, attaching event listener');
        filterSelect.addEventListener('change', function() {
            console.log('Filter changed:', this.value);
            filterModules();
        });
    }
    
    // Initial filter run
    filterModules();
}