(function () {
    "use strict";

    const MESSAGE_FACTORIES = {
        valueMissing: field => field.dataset.errorRequired || "This field is required.",
        typeMismatch: field => field.dataset.errorType || (field.type === "email" ? "Please enter a valid email address." : "Please enter a valid value."),
        patternMismatch: field => field.dataset.errorPattern || "Please match the requested format.",
        tooShort: field => field.dataset.errorMinlength || `Please enter at least ${field.getAttribute("minlength")} characters.`,
        tooLong: field => field.dataset.errorMaxlength || `Please enter no more than ${field.getAttribute("maxlength")} characters.`,
        rangeUnderflow: field => field.dataset.errorMin || `Please enter a value greater than or equal to ${field.getAttribute("min")}.`,
        rangeOverflow: field => field.dataset.errorMax || `Please enter a value less than or equal to ${field.getAttribute("max")}.`,
        stepMismatch: field => field.dataset.errorStep || "Please enter a valid value.",
        badInput: field => field.dataset.errorType || "Please enter a valid value."
    };

    function findMessageTarget(field) {
        const targetId = field.getAttribute("data-error-target");
        if (targetId) {
            return document.getElementById(targetId);
        }

        const fieldWrapper = field.closest("[data-field]");
        if (fieldWrapper) {
            const labelled = fieldWrapper.querySelector("[data-error-message], .field__error, .error-message");
            if (labelled) {
                return labelled;
            }
        }

        const sibling = field.nextElementSibling;
        if (sibling && (sibling.hasAttribute("data-error-message") || sibling.classList.contains("field__error") || sibling.classList.contains("error-message"))) {
            return sibling;
        }

        return null;
    }

    function clearMessage(field) {
        field.removeAttribute("aria-invalid");
        const target = findMessageTarget(field);
        if (target) {
            target.textContent = "";
        }
    }

    function showMessage(field, message) {
        field.setAttribute("aria-invalid", "true");
        const target = findMessageTarget(field);
        if (target) {
            target.textContent = message;
        }
    }

    function messageFor(field) {
        const { validity } = field;
        for (const key in MESSAGE_FACTORIES) {
            if (Object.prototype.hasOwnProperty.call(MESSAGE_FACTORIES, key) && validity[key]) {
                return MESSAGE_FACTORIES[key](field);
            }
        }
        return field.validationMessage || "The value provided is not valid.";
    }

    function isSupportedField(field) {
        return field instanceof HTMLInputElement || field instanceof HTMLSelectElement || field instanceof HTMLTextAreaElement;
    }

    function updateFieldState(field) {
        if (!(field instanceof HTMLElement) || !isSupportedField(field)) {
            return true;
        }

        if (field.matches(":disabled") || field.getAttribute("aria-disabled") === "true") {
            clearMessage(field);
            return true;
        }

        if (field.validity.valid) {
            clearMessage(field);
            return true;
        }

        showMessage(field, messageFor(field));
        return false;
    }

    function collectFields(form) {
        return Array.from(form.querySelectorAll("input, select, textarea"));
    }

    function updatePreview(form) {
        const preview = document.getElementById("order-preview");
        if (!preview) {
            return;
        }

        const formData = new FormData(form);
        const entries = Array.from(formData.entries());
        if (entries.length === 0) {
            preview.textContent = "";
            preview.hidden = true;
            return;
        }

        preview.textContent = entries.map(([key, value]) => `${key}: ${value}`).join("\n");
        preview.hidden = false;
    }

    function handleSubmit(event) {
        const form = event.currentTarget;
        if (!(form instanceof HTMLFormElement)) {
            return;
        }

        const fields = collectFields(form);
        let hasErrors = false;
        fields.forEach(field => {
            const valid = updateFieldState(field);
            if (!valid) {
                hasErrors = true;
            }
        });

        if (hasErrors) {
            event.preventDefault();
            const firstInvalid = fields.find(field => field.getAttribute("aria-invalid") === "true");
            if (firstInvalid) {
                firstInvalid.focus();
            }
            return;
        }

        updatePreview(form);
    }

    function attachValidation(form) {
        const fields = collectFields(form);
        fields.forEach(field => {
            const handler = () => updateFieldState(field);
            field.addEventListener("input", handler);
            field.addEventListener("blur", handler);
        });

        form.addEventListener("submit", handleSubmit);
        form.addEventListener("reset", () => {
            window.requestAnimationFrame(() => {
                fields.forEach(clearMessage);
                const preview = document.getElementById("order-preview");
                if (preview) {
                    preview.textContent = "";
                    preview.hidden = true;
                }
            });
        });
    }

    function initialise() {
        document.querySelectorAll("form[data-validate]").forEach(element => {
            if (element instanceof HTMLFormElement) {
                element.setAttribute("novalidate", "novalidate");
                attachValidation(element);
            }
        });
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", initialise);
    } else {
        initialise();
    }
})();
