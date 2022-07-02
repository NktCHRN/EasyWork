import {AbstractControl, ValidationErrors, ValidatorFn} from '@angular/forms';
import { BooleanContainer } from './booleancontainer';

export function createNotWhitespaceValidator(): ValidatorFn {
    return (control:AbstractControl) : ValidationErrors | null => {
        const value = control.value;

        if (!value) {
            return null;
        }

        const isWhitespace = control.value.trim().length === 0;

        return !isWhitespace ? null: {notWhitespace:true};
    }
}

export function createIsDusplicateValidator(isDuplicate: BooleanContainer): ValidationErrors
{
    return (control:AbstractControl) : ValidationErrors | null => {
        return !isDuplicate.value ? null : {notDuplicate : true};
    }
}

export function createHasNumbericValidator(): ValidatorFn {
    return (control:AbstractControl) : ValidationErrors | null => {
        const value = control.value;

        if (!value) {
            return null;
        }

        const hasNumeric = /[0-9]+/.test(value);;

        return hasNumeric ? null : {hasNumberic:false};
    }
}

export function createHasLowerCaseValidator(): ValidatorFn {
    return (control:AbstractControl) : ValidationErrors | null => {
        const value = control.value;

        if (!value) {
            return null;
        }

        const hasLowerCase = /[a-z]+/.test(value);

        return hasLowerCase ? null: {hasLowerCase:false};
    }
}

export function createHasUpperCaseValidator(): ValidatorFn {
    return (control:AbstractControl) : ValidationErrors | null => {
        const value = control.value;

        if (!value) {
            return null;
        }

        const hasUpperCase = /[A-Z]+/.test(value);

        return hasUpperCase ? null: {hasUpperCase:false};
    }
}

export function createIsEqualToValidator(matchTo: string): ValidatorFn {
    return (control:AbstractControl) : ValidationErrors | null => {
        const value = control.value;

        if (!value) {
            return null;
        }
        return !!control.parent &&
        !!control.parent.value &&
        value === control.parent.get(matchTo)!.value
        ? null
        : {matches: false}
    }
}
