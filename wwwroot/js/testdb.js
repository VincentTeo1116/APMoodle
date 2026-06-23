document.addEventListener('DOMContentLoaded', function() {
    const cards = document.querySelectorAll('.database-stat-card[data-table]');
    const modal = document.getElementById('terminalModal');
    const closeBtn = document.getElementById('terminalCloseBtn');
    const modalClose = document.getElementById('terminalModalClose');
    const modalTitle = document.getElementById('modalTitle');
    const terminalOutput = document.getElementById('terminalOutput');

    function closeModal() {
        modal.style.display = 'none';
    }

    closeBtn.addEventListener('click', closeModal);
    modalClose.addEventListener('click', closeModal);
    modal.addEventListener('click', function(e) {
        if (e.target === modal) closeModal();
    });
    document.addEventListener('keydown', function(e) {
        if (e.key === 'Escape') closeModal();
    });

    cards.forEach(card => {
        card.addEventListener('click', async function() {
            const table = this.getAttribute('data-table');
            const tableDisplay = table.charAt(0).toUpperCase() + table.slice(1);
            modalTitle.textContent = `${tableDisplay.toUpperCase()} TABLE DATA`;
            terminalOutput.innerHTML = `<span class="sql-comment">Loading data from ${tableDisplay}...</span>`;
            modal.style.display = 'flex';

            try {
                const response = await fetch(`?handler=Data&table=${table}`);
                const result = await response.json();

                if (!result.success) {
                    terminalOutput.innerHTML = `<span style="color:#f87171;">ERROR: ${result.message}</span>`;
                    return;
                }

                const data = result.data;
                if (!data || data.length === 0) {
                    terminalOutput.innerHTML = `<span style="color:#fbbf24;">NO records found in ${tableDisplay}.</span>`;
                    return;
                }

                let html = `<div class="sql-query">SELECT * FROM ${tableDisplay};</div>`;
                html += `<div class="result-row-count">>> ${data.length} row(s) returned</div>`;
                const columns = Object.keys(data[0]);
                html += `<table class="result-table"><thead><tr>`;
                columns.forEach(col => { html += `<th>${escapeHtml(col)}</th>`; });
                html += `</tr></thead><tbody>`;
                data.forEach(row => {
                    html += `<tr>`;
                    columns.forEach(col => {
                        let value = row[col];
                        if (value === null || value === undefined) value = 'NULL';
                        else if (typeof value === 'object') value = JSON.stringify(value).substring(0, 40) + '…';
                        else if (typeof value === 'string' && value.length > 30) value = value.substring(0, 30) + '…';
                        html += `<td>${escapeHtml(value)}</td>`;
                    });
                    html += `</tr>`;
                });
                html += `</tbody></table>`;
                if (data.length >= 100) {
                    html += `<div class="result-row-count" style="color:#fbbf24;">ALERT: Showing first 100 rows.</div>`;
                }
                terminalOutput.innerHTML = html;

            } catch (error) {
                terminalOutput.innerHTML = `<span style="color:#f87171;">ERROR: Network error: ${error.message}</span>`;
            }
        });
    });

    function escapeHtml(text) {
        if (text === null || text === undefined) return 'NULL';
        const div = document.createElement('div');
        div.textContent = String(text);
        return div.innerHTML;
    }
});