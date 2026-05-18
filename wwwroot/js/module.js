function regenerateCode(moduleId) {
    if (!confirm('Generate a new invitation code? The previous code will be invalidated.')) {
        return;
    }

    const submitBtn = document.querySelector('.regenerate-btn');
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