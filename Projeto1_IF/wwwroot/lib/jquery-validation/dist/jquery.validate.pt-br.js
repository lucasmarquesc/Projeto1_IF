//Lucas Marques da Cunha
jQuery.extend(jQuery.validator.methods, {
    date: function (value, element) {
        return this.optional(element) || /^\d\d?\/\d\d?\/\d\d\d?\d?$/.test(value);
    },
    number: function (value, element) {
        return this.optional(element) || /^-?(?:\d+|\d{1,3}(?:\.\d{3})+)(?:,\d+)?$/.test(value);
    },
    cpf: function (value, element) { //Validar CPF
        return /^(\d{3}\.\d{3}\.\d{3}-\d{2}|\d{11})$/.test(value);
    }
});
