(() => {
    "use strict";

    const isFormControl = (element) =>
        element instanceof HTMLInputElement ||
        element instanceof HTMLSelectElement ||
        element instanceof HTMLTextAreaElement;

    const applyCustomValidation = (form) => {
        const confirmFields = form.querySelectorAll('[data-confirm-with]');
        confirmFields.forEach((field) => {
            if (!isFormControl(field)) {
                return;
            }

            const targetName = field.getAttribute("data-confirm-with");
            if (!targetName) {
                return;
            }

            const targetField = form.querySelector(`[name="${CSS.escape(targetName)}"]`);
            if (!isFormControl(targetField)) {
                return;
            }

            if (field.value === targetField.value) {
                field.setCustomValidity("");
                return;
            }

            const message = field.getAttribute("data-confirm-message") ?? "The values do not match.";
            field.setCustomValidity(message);
        });
    };

    const setMessage = (form, field) => {
        const messageTarget = form.querySelector(`[data-valmsg-for="${CSS.escape(field.name)}"]`);
        if (!messageTarget) {
            return;
        }

        if (field.validity.valid) {
            messageTarget.textContent = "";
            messageTarget.classList.remove("field-validation-error");
            messageTarget.classList.add("field-validation-valid");
            return;
        }

        messageTarget.textContent = field.validationMessage;
        messageTarget.classList.remove("field-validation-valid");
        messageTarget.classList.add("field-validation-error");
    };

    const showSummary = (form) => {
        const summary = form.querySelector('[data-validation-summary="true"]');
        if (!summary) {
            return;
        }

        if (!summary.textContent?.trim()) {
            summary.textContent = "Please correct the errors below and try again.";
        }
    };

    window.addEventListener("load", () => {
        const forms = document.querySelectorAll('form[data-validate="true"]');
        forms.forEach((form) => {
            form.addEventListener("submit", (event) => {
                applyCustomValidation(form);
                if (form.checkValidity()) {
                    return;
                }

                event.preventDefault();
                event.stopPropagation();
                showSummary(form);

                const fields = Array.from(form.elements).filter(isFormControl);
                fields.forEach((field) => setMessage(form, field));

                const firstInvalid = fields.find((field) => !field.validity.valid);
                if (firstInvalid) {
                    firstInvalid.focus();
                }
            });

            form.addEventListener("input", (event) => {
                const target = event.target;
                if (!isFormControl(target)) {
                    return;
                }

                applyCustomValidation(form);
                setMessage(form, target);

                const dependents = form.querySelectorAll(`[data-confirm-with="${CSS.escape(target.name)}"]`);
                dependents.forEach((dependent) => {
                    if (!isFormControl(dependent)) {
                        return;
                    }

                    applyCustomValidation(form);
                    setMessage(form, dependent);
                });
            });
        });
    });
})();
