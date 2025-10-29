(function () {
    "use strict";

    const VALIDITY_MAP = [
        { key: "valueMissing", dataKey: "errorRequired", defaultMessage: "This field is required." },
        { key: "typeMismatch", dataKey: "errorType", defaultMessage: field => field.type === "email" ? "Please enter a valid email address." : "Please enter a valid value." },
        { key: "patternMismatch", dataKey: "errorPattern", defaultMessage: "Please match the requested format." },
        { key: "tooShort", dataKey: "errorMinlength", defaultMessage: field => `Please enter at least ${field.getAttribute("minlength")} characters.` },
        { key: "tooLong", dataKey: "errorMaxlength", defaultMessage: field => `Please enter no more than ${field.getAttribute("maxlength")} characters.` },
        { key: "rangeUnderflow", dataKey: "errorMin", defaultMessage: field => `Please enter a value greater than or equal to ${field.getAttribute("min")}.` },
        { key: "rangeOverflow", dataKey: "errorMax", defaultMessage: field => `Please enter a value less than or equal to ${field.getAttribute("max")}.` },
        { key: "stepMismatch", dataKey: "errorStep", defaultMessage: "Please enter a valid value." },
        { key: "badInput", dataKey: "errorType", defaultMessage: "Please enter a valid value." }
    ];

    function getMessage(field, descriptor) {
        const customMessage = field.dataset?.[descriptor.dataKey];

        if (customMessage) {
            return customMessage;
        }

        if (typeof descriptor.defaultMessage === "function") {
            return descriptor.defaultMessage(field);
        }

        return descriptor.defaultMessage;
    }

    function findMessageElement(field) {
        const targetId = field.getAttribute("data-error-target");

        if (targetId) {
            return document.getElementById(targetId);
        }

        const labelled = field.closest("[data-field]");

        if (labelled) {
            const candidate = labelled.querySelector("[data-error-message], .field__error, .error-message");
            if (candidate) {
                return candidate;
            }
        }

        const next = field.nextElementSibling;
        if (next && (next.hasAttribute("data-error-message") || next.classList.contains("field__error") || next.classList.contains("error-message"))) {
            return next;
        }

        return null;
    }

    function clearError(field) {
        field.removeAttribute("aria-invalid");

        const messageElement = findMessageElement(field);
        if (messageElement) {
            messageElement.textContent = "";
        }
    }

    function showError(field, message) {
        field.setAttribute("aria-invalid", "true");

        const messageElement = findMessageElement(field);
        if (messageElement) {
            messageElement.textContent = message;
        }
    }

    function validateField(field) {
        if (!(field instanceof HTMLElement)) {
            return true;
        }

        const element = field;
        const isDisabled = element.matches(":disabled") || element.getAttribute("aria-disabled") === "true";

        if (isDisabled) {
            clearError(element);
            return true;
        }

        if (!(element instanceof HTMLInputElement || element instanceof HTMLSelectElement || element instanceof HTMLTextAreaElement)) {
            return true;
        }

        const validity = element.validity;

        if (validity.valid) {
            clearError(element);
            return true;
        }

        for (const descriptor of VALIDITY_MAP) {
            if (validity[descriptor.key]) {
                const message = getMessage(element, descriptor);
                showError(element, message);
                return false;
            }
        }

        showError(element, element.validationMessage || "The value provided is not valid.");
        return false;
    }

    function validateForm(form) {
        if (!(form instanceof HTMLFormElement)) {
            return true;
        }

        const fields = Array.from(form.querySelectorAll("input, select, textarea"));
        let isValid = true;

        for (const field of fields) {
            const fieldValid = validateField(field);
            if (!fieldValid && isValid) {
                isValid = false;
            }
        }

        return isValid;
    }

    function handleSubmit(event) {
        const form = event.currentTarget;
        if (!(form instanceof HTMLFormElement)) {
            return;
        }

        if (!validateForm(form)) {
            event.preventDefault();
            const firstInvalid = form.querySelector("[aria-invalid='true']");
            if (firstInvalid instanceof HTMLElement) {
                firstInvalid.focus();
            }
            return;
        }

        const preview = document.getElementById("order-preview");
        if (preview) {
            const formData = new FormData(form);
            const entries = Array.from(formData.entries()).map(([key, value]) => `${key}: ${value}`);
            preview.textContent = entries.join("\n");
            preview.hidden = entries.length === 0;
        }
    }

    function attachRealtimeValidation(form) {
        const fields = Array.from(form.querySelectorAll("input, select, textarea"));

        for (const field of fields) {
            const handler = () => validateField(field);
            field.addEventListener("input", handler);
            field.addEventListener("blur", handler);
        }

        form.addEventListener("submit", handleSubmit);
        form.addEventListener("reset", () => {
            window.requestAnimationFrame(() => {
                for (const field of fields) {
                    clearError(field);
                }
                const preview = document.getElementById("order-preview");
                if (preview) {
                    preview.textContent = "";
                    preview.hidden = true;
                }
            });
        });
    }

    function initialise() {
        const forms = document.querySelectorAll("form[data-validate]");
        forms.forEach(form => {
            if (form instanceof HTMLFormElement) {
                form.setAttribute("novalidate", "novalidate");
                attachRealtimeValidation(form);
            }
        });
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", initialise);
    } else {
        initialise();
    }

    window.SimpleFormValidator = {
        validateField,
        validateForm
    };
})();
