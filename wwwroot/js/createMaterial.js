// CreateMaterial.js - Form Handler
(function() {
    function initForm() {
        const moduleIdInput = document.querySelector('input[name="moduleId"]');
        if (!moduleIdInput) return;
        
        const moduleId = moduleIdInput.value;
        
        const fileTypeBtn = document.getElementById('fileTypeBtn');
        const linkTypeBtn = document.getElementById('linkTypeBtn');
        const textTypeBtn = document.getElementById('textTypeBtn');
        const fileSection = document.getElementById('fileSection');
        const linkSection = document.getElementById('linkSection');
        const textSection = document.getElementById('textSection');
        
        let selectedFile = null;
        let selectedContentType = 'file';
        
        function setActiveType(type) {
            selectedContentType = type;
            [fileTypeBtn, linkTypeBtn, textTypeBtn].forEach(btn => {
                if (btn) btn.classList.remove('active');
            });
            if (type === 'file') fileTypeBtn.classList.add('active');
            if (type === 'link') linkTypeBtn.classList.add('active');
            if (type === 'text') textTypeBtn.classList.add('active');
            
            if (fileSection) fileSection.style.display = type === 'file' ? 'block' : 'none';
            if (linkSection) linkSection.style.display = type === 'link' ? 'block' : 'none';
            if (textSection) textSection.style.display = type === 'text' ? 'block' : 'none';
        }
        
        if (fileTypeBtn) fileTypeBtn.addEventListener('click', () => setActiveType('file'));
        if (linkTypeBtn) linkTypeBtn.addEventListener('click', () => setActiveType('link'));
        if (textTypeBtn) textTypeBtn.addEventListener('click', () => setActiveType('text'));
        
        // File upload handling
        const dropZone = document.getElementById('dropZone');
        const fileInput = document.getElementById('fileInput');
        const filePreview = document.getElementById('filePreview');
        const previewName = document.getElementById('previewName');
        const previewSize = document.getElementById('previewSize');
        const previewIcon = document.getElementById('previewIcon');
        
        if (dropZone) {
            dropZone.addEventListener('click', () => fileInput.click());
            dropZone.addEventListener('dragover', (e) => { e.preventDefault(); dropZone.classList.add('drag-over'); });
            dropZone.addEventListener('dragleave', () => { dropZone.classList.remove('drag-over'); });
            dropZone.addEventListener('drop', (e) => {
                e.preventDefault();
                dropZone.classList.remove('drag-over');
                if (e.dataTransfer.files.length > 0) handleFileSelect(e.dataTransfer.files[0]);
            });
        }
        
        if (fileInput) {
            fileInput.addEventListener('change', (e) => { if (e.target.files.length > 0) handleFileSelect(e.target.files[0]); });
        }
        
        function handleFileSelect(file) {
            selectedFile = file;
            if (previewName) previewName.textContent = file.name;
            if (previewSize) previewSize.textContent = formatFileSize(file.size);
            const ext = file.name.split('.').pop().toLowerCase();
            if (previewIcon) {
                if (ext === 'pdf') previewIcon.textContent = '📄';
                else if (['mp4', 'mov', 'avi'].includes(ext)) previewIcon.textContent = '🎥';
                else if (['jpg', 'jpeg', 'png', 'gif'].includes(ext)) previewIcon.textContent = '🖼️';
                else previewIcon.textContent = '📘';
            }
            if (dropZone) dropZone.style.display = 'none';
            if (filePreview) filePreview.style.display = 'flex';
        }
        
        window.clearFile = function() { 
            selectedFile = null; 
            if (fileInput) fileInput.value = ''; 
            if (dropZone) dropZone.style.display = 'block'; 
            if (filePreview) filePreview.style.display = 'none'; 
        };
        
        // URL handling
        const urlInput = document.getElementById('contentUrl');
        const urlPreview = document.getElementById('urlPreview');
        const previewContent = document.getElementById('previewContent');
        
        if (urlInput) {
            urlInput.addEventListener('input', function() {
                const url = this.value;
                if (url && urlPreview && previewContent) {
                    const embedHtml = getEmbedHtml(url);
                    if (embedHtml) {
                        previewContent.innerHTML = embedHtml;
                        urlPreview.style.display = 'block';
                    } else {
                        previewContent.innerHTML = `<a href="${url}" target="_blank" class="external-link">🔗 Open Link in New Tab</a>`;
                        urlPreview.style.display = 'block';
                    }
                } else if (urlPreview) {
                    urlPreview.style.display = 'none';
                }
            });
        }
        
        function getEmbedHtml(url) {
            if (url.includes('youtube.com/watch') || url.includes('youtu.be')) {
                let videoId;
                if (url.includes('youtube.com/watch')) {
                    videoId = url.split('v=')[1]?.split('&')[0];
                } else {
                    videoId = url.split('/').pop();
                }
                return `<iframe src="https://www.youtube.com/embed/${videoId}" frameborder="0" allowfullscreen></iframe>`;
            }
            if (url.includes('drive.google.com/file')) {
                const fileId = url.match(/\/d\/(.+?)\//)?.[1];
                if (fileId) {
                    return `<iframe src="https://drive.google.com/file/d/${fileId}/preview" frameborder="0" allowfullscreen></iframe>`;
                }
            }
            return null;
        }
        
        window.clearUrl = function() { 
            if (urlInput) urlInput.value = ''; 
            if (urlPreview) urlPreview.style.display = 'none'; 
        };
        
        function formatFileSize(bytes) {
            if (bytes === 0) return '0 Bytes';
            const k = 1024;
            const sizes = ['Bytes', 'KB', 'MB', 'GB'];
            const i = Math.floor(Math.log(bytes) / Math.log(k));
            return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
        }
        
        const form = document.getElementById('materialForm');
        if (form) {
            form.addEventListener('submit', async (e) => {
                e.preventDefault();
                const title = document.getElementById('title')?.value;
                const description = document.getElementById('description')?.value;
                
                if (!title) { alert('Please enter a title'); return; }
                
                const formData = new FormData();
                formData.append('moduleId', moduleId);
                formData.append('title', title);
                formData.append('description', description || '');
                formData.append('contentType', selectedContentType);
                
                if (selectedContentType === 'file') {
                    if (!selectedFile) { alert('Please select a file'); return; }
                    formData.append('file', selectedFile);
                } else if (selectedContentType === 'link') {
                    const url = document.getElementById('contentUrl')?.value;
                    if (!url) { alert('Please enter a URL'); return; }
                    formData.append('contentUrl', url);
                } else if (selectedContentType === 'text') {
                    const text = document.getElementById('contentText')?.value;
                    if (!text) { alert('Please enter text content'); return; }
                    formData.append('contentText', text);
                }
                
                const submitBtn = document.querySelector('.btn-submit');
                if (submitBtn) {
                    submitBtn.disabled = true;
                    submitBtn.textContent = 'Uploading...';
                }
                
                try {
                    const response = await fetch('/FrontEnd/CreateMaterial', {
                        method: 'POST',
                        body: formData
                    });
                    if (response.ok) {
                        window.location.href = `/FrontEnd/TeachingMaterial/${moduleId}`;
                    } else {
                        const error = await response.text();
                        alert(`Upload failed: ${error}`);
                        if (submitBtn) {
                            submitBtn.disabled = false;
                            submitBtn.textContent = '📤 Upload';
                        }
                    }
                } catch (error) {
                    alert(`Upload failed: ${error.message}`);
                    if (submitBtn) {
                        submitBtn.disabled = false;
                        submitBtn.textContent = '📤 Upload';
                    }
                }
            });
        }
    }
    
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initForm);
    } else {
        initForm();
    }
})();