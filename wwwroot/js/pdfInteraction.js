/**
 * PDF Interaction JS
 * 
 * This script handles the interaction with PDF forms embedded in the application.
 * It uses PDF.js to render the PDF and capture form field changes.
 */

class PdfInteraction {
    constructor(containerId, pdfUrl, reportId, userId, isReadOnly = false) {
        this.containerId = containerId;
        this.pdfUrl = pdfUrl;
        this.reportId = reportId;
        this.userId = userId;
        this.isReadOnly = isReadOnly;
        this.formFields = {};
        this.container = document.getElementById(containerId);
        this.pdfDoc = null;
        this.pageNum = 1;
        this.pageRendering = false;
        this.pageNumPending = null;
        this.scale = 1.2;
        
        if (!this.container) {
            console.error(`Container with ID ${containerId} not found`);
            return;
        }
        
        this.init();
    }
    
    async init() {
        try {
            // Create PDF viewer container elements
            this.createViewerElements();
            
            // Load the PDF
            await this.loadPdf();
            
            // Setup event listeners
            this.setupEventListeners();
        } catch (error) {
            this.showError(`Error initializing PDF viewer: ${error.message}`);
            console.error('PDF Interaction init error:', error);
        }
    }
    
    createViewerElements() {
        // Create main container for PDF and form fields
        this.container.innerHTML = `
            <div class="pdf-container">
                <div class="pdf-controls">
                    <button id="prev-page" class="btn btn-sm btn-outline-primary">Previous</button>
                    <span id="page-num"></span> / <span id="page-count"></span>
                    <button id="next-page" class="btn btn-sm btn-outline-primary">Next</button>
                </div>
                <div class="pdf-viewer">
                    <canvas id="pdf-canvas"></canvas>
                </div>
                <div id="form-fields-container" class="form-fields-container mt-3">
                    <h5>Form Fields</h5>
                    <div id="extracted-fields"></div>
                </div>
                <div class="mt-3">
                    <button id="save-pdf" class="btn btn-primary">Save Changes</button>
                    <div id="save-status" class="mt-2"></div>
                </div>
            </div>
        `;
        
        this.canvas = document.getElementById('pdf-canvas');
        this.ctx = this.canvas.getContext('2d');
        this.pageNumDisplay = document.getElementById('page-num');
        this.pageCountDisplay = document.getElementById('page-count');
        this.extractedFieldsContainer = document.getElementById('extracted-fields');
        this.saveButton = document.getElementById('save-pdf');
        this.saveStatus = document.getElementById('save-status');
        
        // If read-only, hide the save button
        if (this.isReadOnly) {
            this.saveButton.style.display = 'none';
        }
    }
    
    async loadPdf() {
        try {
            // Load the PDF document
            const loadingTask = pdfjsLib.getDocument(this.pdfUrl);
            this.pdfDoc = await loadingTask.promise;
            
            // Display total pages
            this.pageCountDisplay.textContent = this.pdfDoc.numPages;
            
            // Render the first page
            await this.renderPage(this.pageNum);
            
            // Extract form fields
            this.extractFormFields();
        } catch (error) {
            this.showError(`Error loading PDF: ${error.message}`);
            console.error('PDF load error:', error);
        }
    }
    
    async renderPage(pageNumber) {
        this.pageRendering = true;
        
        try {
            // Get the page
            const page = await this.pdfDoc.getPage(pageNumber);
            
            // Set viewport
            const viewport = page.getViewport({ scale: this.scale });
            this.canvas.height = viewport.height;
            this.canvas.width = viewport.width;
            
            // Render the page
            const renderContext = {
                canvasContext: this.ctx,
                viewport: viewport
            };
            
            await page.render(renderContext).promise;
            
            // Update page number display
            this.pageNumDisplay.textContent = pageNumber;
            
            this.pageRendering = false;
            
            // If there's a pending page render, do it now
            if (this.pageNumPending !== null) {
                const pendingPage = this.pageNumPending;
                this.pageNumPending = null;
                this.renderPage(pendingPage);
            }
        } catch (error) {
            this.pageRendering = false;
            this.showError(`Error rendering page ${pageNumber}: ${error.message}`);
            console.error('Page render error:', error);
        }
    }
    
    queueRenderPage(pageNum) {
        if (this.pageRendering) {
            this.pageNumPending = pageNum;
        } else {
            this.renderPage(pageNum);
        }
    }
    
    extractFormFields() {
        // In a real implementation, this would extract form fields from the PDF
        // For this example, we'll create dummy fields based on the TPS report we saw
        
        const fields = [
            { label: 'Date', type: 'text', id: 'date' },
            { label: 'Time (Start)', type: 'time', id: 'time_start' },
            { label: 'Time (End)', type: 'time', id: 'time_end' },
            { label: 'Location', type: 'text', id: 'location' },
            
            // Checkboxes for emotional state
            { label: 'Matt is calm/relaxed', type: 'checkbox', id: 'matt_calm' },
            { label: 'Mina is calm/relaxed', type: 'checkbox', id: 'mina_calm' },
            { label: 'Matt is anxious/nervous', type: 'checkbox', id: 'matt_anxious' },
            { label: 'Mina is anxious/nervous', type: 'checkbox', id: 'mina_anxious' },
            
            // Physical conditions
            { label: 'Matt feels good physically', type: 'checkbox', id: 'matt_good_physical' },
            { label: 'Mina feels good physically', type: 'checkbox', id: 'mina_good_physical' },
            
            // Notes fields
            { label: "Matt's Notes", type: 'textarea', id: 'matt_notes' },
            { label: "Mina's Notes", type: 'textarea', id: 'mina_notes' },
            
            // Signatures
            { label: "Matt's Initials", type: 'text', id: 'matt_initials' },
            { label: "Mina's Initials", type: 'text', id: 'mina_initials' }
        ];
        
        // Clear the container
        this.extractedFieldsContainer.innerHTML = '';
        
        // Create UI elements for each field
        fields.forEach(field => {
            switch (field.type) {
                case 'checkbox':
                    this.createCheckboxField(this.extractedFieldsContainer, field.label, field.id);
                    break;
                case 'textarea':
                    this.createTextAreaField(this.extractedFieldsContainer, field.label, field.id);
                    break;
                default:
                    this.createFormField(this.extractedFieldsContainer, field.label, field.type, field.id);
                    break;
            }
        });
        
        // If read-only, disable all fields
        if (this.isReadOnly) {
            const inputs = this.extractedFieldsContainer.querySelectorAll('input, textarea');
            inputs.forEach(input => {
                input.disabled = true;
            });
        }
    }
    
    createFormField(container, label, type, id) {
        const fieldContainer = document.createElement('div');
        fieldContainer.className = 'mb-3';
        
        const labelElement = document.createElement('label');
        labelElement.textContent = label;
        labelElement.htmlFor = id;
        labelElement.className = 'form-label';
        
        const input = document.createElement('input');
        input.type = type;
        input.id = id;
        input.name = id;
        input.className = 'form-control';
        input.addEventListener('change', e => this.handleFieldChange(e));
        
        fieldContainer.appendChild(labelElement);
        fieldContainer.appendChild(input);
        container.appendChild(fieldContainer);
        
        // Store reference to the field
        this.formFields[id] = '';
    }
    
    createCheckboxField(container, label, id) {
        const fieldContainer = document.createElement('div');
        fieldContainer.className = 'form-check mb-2';
        
        const input = document.createElement('input');
        input.type = 'checkbox';
        input.id = id;
        input.name = id;
        input.className = 'form-check-input';
        input.addEventListener('change', e => this.handleFieldChange(e));
        
        const labelElement = document.createElement('label');
        labelElement.textContent = label;
        labelElement.htmlFor = id;
        labelElement.className = 'form-check-label';
        
        fieldContainer.appendChild(input);
        fieldContainer.appendChild(labelElement);
        container.appendChild(fieldContainer);
        
        // Store reference to the field
        this.formFields[id] = false;
    }
    
    createTextAreaField(container, label, id) {
        const fieldContainer = document.createElement('div');
        fieldContainer.className = 'mb-3';
        
        const labelElement = document.createElement('label');
        labelElement.textContent = label;
        labelElement.htmlFor = id;
        labelElement.className = 'form-label';
        
        const textarea = document.createElement('textarea');
        textarea.id = id;
        textarea.name = id;
        textarea.className = 'form-control';
        textarea.rows = 3;
        textarea.addEventListener('change', e => this.handleFieldChange(e));
        
        fieldContainer.appendChild(labelElement);
        fieldContainer.appendChild(textarea);
        container.appendChild(fieldContainer);
        
        // Store reference to the field
        this.formFields[id] = '';
    }
    
    handleFieldChange(event) {
        const target = event.target;
        const value = target.type === 'checkbox' ? target.checked : target.value;
        const name = target.name;
        
        this.formFields[name] = value;
        console.log(`Field ${name} changed to ${value}`);
    }
    
    async saveFormData() {
        try {
            this.saveStatus.textContent = 'Saving...';
            this.saveStatus.className = 'mt-2 text-info';
            
            // Prepare data to send
            const data = {
                reportId: this.reportId,
                userId: this.userId,
                fields: this.formFields
            };
            
            // Send data to server
            const response = await fetch('/api/tpsreport/savefields', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(data)
            });
            
            if (response.ok) {
                const result = await response.json();
                this.saveStatus.textContent = 'Form saved successfully!';
                this.saveStatus.className = 'mt-2 text-success';
                
                // If there's a redirect URL, navigate there after a delay
                if (result.redirectUrl) {
                    setTimeout(() => {
                        window.location.href = result.redirectUrl;
                    }, 1500);
                }
            } else {
                const error = await response.json();
                this.showError(`Error saving form: ${error.message}`);
            }
        } catch (error) {
            this.showError(`Error saving form: ${error.message}`);
            console.error('Save error:', error);
        }
    }
    
    showError(message) {
        this.saveStatus.textContent = message;
        this.saveStatus.className = 'mt-2 text-danger';
    }
    
    setupEventListeners() {
        // Previous page button
        document.getElementById('prev-page').addEventListener('click', () => {
            if (this.pageNum <= 1) return;
            this.pageNum--;
            this.queueRenderPage(this.pageNum);
        });
        
        // Next page button
        document.getElementById('next-page').addEventListener('click', () => {
            if (this.pageNum >= this.pdfDoc.numPages) return;
            this.pageNum++;
            this.queueRenderPage(this.pageNum);
        });
        
        // Save button
        if (!this.isReadOnly) {
            this.saveButton.addEventListener('click', () => this.saveFormData());
        }
    }
}

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', function() {
    const pdfContainer = document.getElementById('pdf-container');
    if (pdfContainer) {
        const pdfUrl = pdfContainer.getAttribute('data-pdf-url');
        const reportId = pdfContainer.getAttribute('data-report-id');
        const userId = pdfContainer.getAttribute('data-user-id');
        const isReadOnly = pdfContainer.getAttribute('data-readonly') === 'true';
        
        if (pdfUrl) {
            new PdfInteraction('pdf-container', pdfUrl, reportId, userId, isReadOnly);
        }
    }
});