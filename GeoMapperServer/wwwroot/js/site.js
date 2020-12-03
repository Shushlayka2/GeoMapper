// -- Common Configurations --
var toastrTimeout = 2000;
const attribution = '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors';
const tileUrl = 'https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png';

const map = L.map('map').setView([0, 0], 1);
L.tileLayer(tileUrl, { attribution }).addTo(map);

$(document).ajaxStart(function () {
    $('#map').fadeTo('slow', 0.33);
});
$(document).ajaxComplete(function () {
    $('#map').fadeTo('slow', 1);
});

// -- Validation Rules on Client Side --
function validator($form) {
    let isValid = true;
    
    //frequency value has to be positive
    if ($form.attr('id') === 'searching_form' && $form.find('input[name=Frequency]')[0].value < 1) {
        $form.find('input[name=Frequency]')[0].classList.add('is-invalid');
        $form.find("span.field-validation-valid[data-valmsg-for='Frequency']").text('Frequency has to be positive');
        isValid = false;
    }

    //"required" attribute behavior handling
    $form.find('input[required]').each(function () {
        if (!this.value) {
            let name = $(this).attr('name');
            this.classList.add('is-invalid');
            $form.find(`span.field-validation-valid[data-valmsg-for='${name}']`).text('The field has to be entered');
            isValid = false;
        }
    });
    return isValid;
}

// -- Polynoms Fetching by Location --
$('#searching_form').submit(function (e) {
    e.preventDefault();
    var $form = $(this);
    $form.find('input.form-control').each(function () { this.classList.remove('is-invalid') });

    if (validator($form)) {
        $.ajax({
            type: 'POST',
            url: this.action,
            data: $(this).serialize(),
            success: function (resp) {
                if (resp.status == 0) {
                    clearMap();
                    let max = 0;
                    if (resp.data.length === 0)
                        toastr.error('Location not found', 'Request Failed', { timeOut: toastrTimeout });
                    else {
                        resp.data.forEach(function (polygon) {
                            var polygonFig = L.polygon(polygon, { color: 'blue' }).addTo(map);
                            // Focus to the biggest area
                            if (polygon.length > max) {
                                max = polygon.length;
                                map.fitBounds(polygonFig.getBounds());
                            }
                        });
                        toastr.success('Requested location uploaded', 'Request Succeed', { timeOut: toastrTimeout });
                    }
                } else {
                    if (resp.modelState) {
                        toastr.error('Invalid request has thrown', 'Request Failed', { timeOut: toastrTimeout });
                        for (field in resp.modelState) {
                            let errs = resp.modelState[field].errors;
                            if (errs.length != 0) {
                                $form.find(`input[name=${field}]`)[0].classList.add('is-invalid');
                                $form.find(`span.field-validation-valid[data-valmsg-for='${field}']`).text(errs[0].errorMessage);
                            }
                        }
                    } else {
                        toastr.error('Oops, something went wrong', 'Request Failed', { timeOut: toastrTimeout });
                    }
                }
            },
            error: function (err) {
                toastr.error("Server doesn't respond", 'Server Error', { timeOut: toastrTimeout });
            }
        });
    }
});

// -- Map Uploading as Image --
$('#download_btn').click(function (e) {
    e.preventDefault();
    var $form = $('#downloading_form');
    $form.find('input.form-control').each(function () { this.classList.remove('is-invalid') });

    if (validator($form)) {
        let mapDiv = document.getElementById('map');
        html2canvas(mapDiv, {
            allowTaint: false,
            useCORS: true
        }).then(
            function (canvas) {
                let dataURL = canvas.toDataURL();
                $form.find('input[name=MapBase64Img]').val(dataURL);
                $form.submit();
            }
        );
    }
});

// Clearing All Poligons from map
function clearMap() {
    for (i in map._layers) {
        if (map._layers[i]._path != undefined) {
            map.removeLayer(map._layers[i]);
        }
    }
}