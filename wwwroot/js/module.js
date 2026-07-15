function regenerateCode(moduleId) {
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
            // Update the inline code display
            const codeElement = document.getElementById('invitationCode');
            if (codeElement) {
                codeElement.textContent = data.code;
                codeElement.style.backgroundColor = '#d4edda';
                codeElement.style.transition = '0.3s';
                setTimeout(() => {
                    codeElement.style.backgroundColor = '';
                }, 500);
            }

            // Show the custom modal with the new code
            showCodeRegenerateModal(data.code);
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

// ─── Show the custom code modal ───
function showCodeRegenerateModal(newCode) {
    const modal = document.getElementById('codeRegenerateModal');
    const codeDisplay = document.getElementById('newInvitationCodeDisplay');
    if (!modal || !codeDisplay) return;

    codeDisplay.textContent = newCode;
    modal.style.display = 'flex';
}

// ─── Copy to clipboard ───
function copyCodeToClipboard() {
    const codeDisplay = document.getElementById('newInvitationCodeDisplay');
    if (!codeDisplay) return;

    const code = codeDisplay.textContent;
    if (!code) return;

    // Use the modern clipboard API if available
    if (navigator.clipboard && navigator.clipboard.writeText) {
        navigator.clipboard.writeText(code)
            .then(() => {
                const btn = document.getElementById('copyCodeBtn');
                const originalText = btn.innerHTML;
                btn.innerHTML = '<i class="bi bi-check-circle"></i> Copied!';
                btn.style.background = '#28a745';
                setTimeout(() => {
                    btn.innerHTML = originalText;
                    btn.style.background = '#b8935a';
                }, 2000);
            })
            .catch(() => {
                fallbackCopyCode(code);
            });
    } else {
        fallbackCopyCode(code);
    }
}

// ─── Fallback copy method (for older browsers) ───
function fallbackCopyCode(text) {
    const textarea = document.createElement('textarea');
    textarea.value = text;
    textarea.style.position = 'fixed';
    textarea.style.opacity = '0';
    document.body.appendChild(textarea);
    textarea.select();
    try {
        document.execCommand('copy');
        const btn = document.getElementById('copyCodeBtn');
        const originalText = btn.innerHTML;
        btn.innerHTML = '<i class="bi bi-check-circle"></i> Copied!';
        btn.style.background = '#28a745';
        setTimeout(() => {
            btn.innerHTML = originalText;
            btn.style.background = '#b8935a';
        }, 2000);
    } catch (err) {
        alert('Failed to copy code. Please select and copy it manually.');
    }
    document.body.removeChild(textarea);
}

// ─── Close the modal ───
function closeCodeRegenerateModal() {
    const modal = document.getElementById('codeRegenerateModal');
    if (modal) {
        modal.style.display = 'none';
    }
}

// ─── Attach event listeners after DOM load ───
document.addEventListener('DOMContentLoaded', function() {
    const copyBtn = document.getElementById('copyCodeBtn');
    if (copyBtn) {
        copyBtn.addEventListener('click', copyCodeToClipboard);
    }

    const closeBtn = document.getElementById('closeCodeModalBtn');
    if (closeBtn) {
        closeBtn.addEventListener('click', closeCodeRegenerateModal);
    }

    // Close modal on overlay click
    const modal = document.getElementById('codeRegenerateModal');
    if (modal) {
        modal.addEventListener('click', function(e) {
            if (e.target === modal) {
                closeCodeRegenerateModal();
            }
        });
    }

    // Close on Escape key
    document.addEventListener('keydown', function(e) {
        if (e.key === 'Escape') {
            const modal = document.getElementById('codeRegenerateModal');
            if (modal && modal.style.display === 'flex') {
                closeCodeRegenerateModal();
            }
        }
    });
});

// Search and Filter functionality
function filterModules() {
    const searchInput = document.getElementById('searchModule');
    const filterSelect = document.getElementById('filterModule');
    
    // If elements don't exist (e.g., guest view), exit
    if (!searchInput || !filterSelect) return;
    
    const searchTerm = searchInput.value.toLowerCase().trim();
    const filterValue = filterSelect.value;
    
    const modules = document.querySelectorAll('.module-card');
    let visibleCount = 0;
    
    modules.forEach(module => {
        const title = module.dataset.title?.toLowerCase() || '';
        const code = module.dataset.code?.toLowerCase() || '';
        const description = module.dataset.description?.toLowerCase() || '';
        const status = module.dataset.status || '';
        
        // Check search match
        const matchesSearch = searchTerm === '' ||
                              title.includes(searchTerm) ||
                              code.includes(searchTerm) ||
                              description.includes(searchTerm);
        
        // Check filter match
        let matchesFilter = true;
        if (filterValue === 'active') {
            matchesFilter = status === 'Active';
        } else if (filterValue === 'completed') {
            matchesFilter = status === 'Completed';
        }
        // 'all' means matchesFilter stays true
        
        if (matchesSearch && matchesFilter) {
            module.style.display = '';
            visibleCount++;
        } else {
            module.style.display = 'none';
        }
    });
    
    // Handle empty results (show no-results message)
    const container = document.querySelector('.PageComponent');
    let noResults = container?.querySelector('.no-results-empty');
    if (visibleCount === 0 && container) {
        if (!noResults) {
            const div = document.createElement('div');
            div.className = 'empty-state no-results-empty';
            div.innerHTML = `
                <span><i class="bi bi-search"></i></span>
                <h3>No matching modules found</h3>
                <p>Try adjusting your search or filter criteria</p>
            `;
            container.appendChild(div);
        }
        // Hide original empty state if it exists (it's the global one when no modules at all)
        const originalEmpty = container.querySelector('.empty-state:not(.no-results-empty)');
        if (originalEmpty) originalEmpty.style.display = 'none';
    } else {
        if (noResults) noResults.remove();

    }
}

// Initialize event listeners - Run when DOM is ready
(function init() {
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', function() {
            setupEventListeners();
        });
    } else {
        setupEventListeners();
    }
})();

function setupEventListeners() {
    const searchInput = document.getElementById('searchModule');
    const filterSelect = document.getElementById('filterModule');
    
    if (searchInput) {
        searchInput.addEventListener('input', filterModules);
    }
    if (filterSelect) {
        filterSelect.addEventListener('change', filterModules);
    }
    
    // Initial filter run
    filterModules();
}