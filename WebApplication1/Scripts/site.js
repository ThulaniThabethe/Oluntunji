// Connect2us Custom JavaScript

$(document).ready(function() {
    // Initialize tooltips
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });

    // Initialize popovers
    var popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'));
    var popoverList = popoverTriggerList.map(function (popoverTriggerEl) {
        return new bootstrap.Popover(popoverTriggerEl);
    });

    // Auto-hide alerts after 5 seconds
    setTimeout(function() {
        $('.alert-dismissible').fadeOut('slow', function() {
            $(this).remove();
        });
    }, 5000);

    // Add loading state to form submissions
    $('form').on('submit', function() {
        var submitBtn = $(this).find('button[type="submit"]');
        if (submitBtn.length) {
            submitBtn.prop('disabled', true);
            submitBtn.html('<span class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>Loading...');
        }
    });

    // Cart functionality
    initializeCartFunctions();

    // Book search functionality
    initializeBookSearch();

    // Form validation enhancements
    initializeFormValidation();

    // Dashboard charts
    initializeDashboardCharts();

    // Image lazy loading
    initializeLazyLoading();

    // Back to top button
    initializeBackToTop();

    // Print functionality
    initializePrintFunctionality();

    // Keyboard shortcuts
    initializeKeyboardShortcuts();
});

// Cart Functions
function initializeCartFunctions() {
    // Add to cart with animation
    $(document).on('click', '.add-to-cart', function(e) {
        e.preventDefault();
        var button = $(this);
        var bookId = button.data('book-id');
        var quantity = button.data('quantity') || 1;

        // Show loading state
        button.prop('disabled', true);
        button.html('<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>');

        $.ajax({
            url: '/Cart/AddToCart',
            type: 'POST',
            data: { bookId: bookId, quantity: quantity },
            success: function(response) {
                if (response.success) {
                    // Update cart count
                    updateCartCount(response.cartItemCount);
                    
                    // Show success message
                    showToast('success', 'Book added to cart successfully!');
                    
                    // Add animation to cart icon
                    animateCartIcon();
                    
                    // Update button state
                    button.removeClass('btn-primary').addClass('btn-success');
                    button.html('<i class="bi bi-check-circle"></i> Added');
                    
                    setTimeout(function() {
                        button.removeClass('btn-success').addClass('btn-primary');
                        button.html('<i class="bi bi-cart-plus"></i> Add to Cart');
                        button.prop('disabled', false);
                    }, 2000);
                } else {
                    showToast('error', response.message || 'Failed to add book to cart');
                    button.prop('disabled', false);
                    button.html('<i class="bi bi-cart-plus"></i> Add to Cart');
                }
            },
            error: function() {
                showToast('error', 'An error occurred while adding the book to cart');
                button.prop('disabled', false);
                button.html('<i class="bi bi-cart-plus"></i> Add to Cart');
            }
        });
    });

    // Update cart quantity
    $(document).on('change', '.cart-quantity', function() {
        var input = $(this);
        var cartItemId = input.data('cart-item-id');
        var newQuantity = input.val();

        $.ajax({
            url: '/Cart/UpdateQuantity',
            type: 'POST',
            data: { cartItemId: cartItemId, quantity: newQuantity },
            success: function(response) {
                if (response.success) {
                    updateCartCount(response.cartItemCount);
                    updateCartTotal(response.cartTotal);
                    showToast('success', 'Cart updated successfully');
                } else {
                    showToast('error', response.message || 'Failed to update cart');
                    input.val(response.originalQuantity);
                }
            },
            error: function() {
                showToast('error', 'An error occurred while updating the cart');
            }
        });
    });

    // Remove from cart
    $(document).on('click', '.remove-from-cart', function(e) {
        e.preventDefault();
        var button = $(this);
        var cartItemId = button.data('cart-item-id');

        if (confirm('Are you sure you want to remove this item from your cart?')) {
            $.ajax({
                url: '/Cart/RemoveFromCart',
                type: 'POST',
                data: { cartItemId: cartItemId },
                success: function(response) {
                    if (response.success) {
                        button.closest('tr').fadeOut(300, function() {
                            $(this).remove();
                            updateCartCount(response.cartItemCount);
                            updateCartTotal(response.cartTotal);
                            
                            if (response.cartItemCount === 0) {
                                location.reload();
                            }
                        });
                        showToast('success', 'Item removed from cart');
                    }
                },
                error: function() {
                    showToast('error', 'An error occurred while removing the item');
                }
            });
        }
    });
}

// Book Search Functions
function initializeBookSearch() {
    var searchTimeout;
    
    $('#bookSearch').on('input', function() {
        clearTimeout(searchTimeout);
        var searchTerm = $(this).val();
        var searchResults = $('#searchResults');
        
        if (searchTerm.length < 2) {
            searchResults.hide();
            return;
        }
        
        searchTimeout = setTimeout(function() {
            $.ajax({
                url: '/Books/SearchBooks',
                type: 'GET',
                data: { searchTerm: searchTerm },
                success: function(response) {
                    if (response.length > 0) {
                        var html = '<div class="list-group">';
                        response.forEach(function(book) {
                            html += '<a href="/Books/Details/' + book.BookId + '" class="list-group-item list-group-item-action">';
                            html += '<div class="d-flex align-items-center">';
                            html += '<img src="' + (book.ImageUrl || '/Content/Images/default-book.jpg') + '" alt="' + book.Title + '" class="me-3" style="width: 50px; height: 70px; object-fit: cover;">';
                            html += '<div>';
                            html += '<h6 class="mb-1">' + book.Title + '</h6>';
                            html += '<p class="mb-1 text-muted">by ' + book.Author + '</p>';
                            html += '<small class="text-success">R' + book.Price.toFixed(2) + '</small>';
                            html += '</div></div></a>';
                        });
                        html += '</div>';
                        searchResults.html(html).show();
                    } else {
                        searchResults.html('<div class="text-muted p-3">No books found</div>').show();
                    }
                },
                error: function() {
                    searchResults.hide();
                }
            });
        }, 300);
    });
    
    // Hide search results when clicking outside
    $(document).on('click', function(e) {
        if (!$(e.target).closest('#bookSearch, #searchResults').length) {
            $('#searchResults').hide();
        }
    });
}

// Form Validation Functions
function initializeFormValidation() {
    // Custom validation for South African phone numbers
    $.validator.addMethod('zaPhoneNumber', function(value, element) {
        return this.optional(element) || /^\+27[0-9]{9}$|^0[0-9]{9}$/.test(value);
    }, 'Please enter a valid South African phone number (e.g., +27123456789 or 0123456789)');

    // Custom validation for South African ID numbers
    $.validator.addMethod('zaIdNumber', function(value, element) {
        return this.optional(element) || /^[0-9]{13}$/.test(value);
    }, 'Please enter a valid 13-digit South African ID number');

    // Custom validation for strong passwords
    $.validator.addMethod('strongPassword', function(value, element) {
        return this.optional(element) || /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$/.test(value);
    }, 'Password must contain at least 8 characters, including uppercase, lowercase, number, and special character');

    // Enhance form validation
    $('form').each(function() {
        $(this).validate({
            errorElement: 'div',
            errorClass: 'invalid-feedback',
            highlight: function(element) {
                $(element).addClass('is-invalid');
            },
            unhighlight: function(element) {
                $(element).removeClass('is-invalid');
            },
            errorPlacement: function(error, element) {
                if (element.parent().hasClass('input-group')) {
                    error.insertAfter(element.parent());
                } else {
                    error.insertAfter(element);
                }
            }
        });
    });
}

// Dashboard Charts Functions
function initializeDashboardCharts() {
    if (typeof Chart !== 'undefined') {
        // Revenue Chart
        var revenueChart = document.getElementById('revenueChart');
        if (revenueChart) {
            new Chart(revenueChart, {
                type: 'line',
                data: {
                    labels: JSON.parse(revenueChart.dataset.labels),
                    datasets: [{
                        label: 'Revenue (R)',
                        data: JSON.parse(revenueChart.dataset.data),
                        borderColor: '#0d6efd',
                        backgroundColor: 'rgba(13, 110, 253, 0.1)',
                        tension: 0.4
                    }]
                },
                options: {
                    responsive: true,
                    plugins: {
                        legend: {
                            display: true,
                            position: 'top'
                        }
                    },
                    scales: {
                        y: {
                            beginAtZero: true,
                            ticks: {
                                callback: function(value) {
                                    return 'R' + value.toLocaleString();
                                }
                            }
                        }
                    }
                }
            });
        }

        // Category Chart
        var categoryChart = document.getElementById('categoryChart');
        if (categoryChart) {
            new Chart(categoryChart, {
                type: 'doughnut',
                data: {
                    labels: JSON.parse(categoryChart.dataset.labels),
                    datasets: [{
                        data: JSON.parse(categoryChart.dataset.data),
                        backgroundColor: [
                            '#007a4d',
                            '#ffb81c',
                            '#de3831',
                            '#002395',
                            '#0d6efd',
                            '#6c757d'
                        ]
                    }]
                },
                options: {
                    responsive: true,
                    plugins: {
                        legend: {
                            position: 'bottom'
                        }
                    }
                }
            });
        }
    }
}

// Lazy Loading Functions
function initializeLazyLoading() {
    if ('IntersectionObserver' in window) {
        var imageObserver = new IntersectionObserver(function(entries, observer) {
            entries.forEach(function(entry) {
                if (entry.isIntersecting) {
                    var img = entry.target;
                    img.src = img.dataset.src;
                    img.classList.remove('lazy');
                    imageObserver.unobserve(img);
                }
            });
        });

        document.querySelectorAll('img[data-src]').forEach(function(img) {
            imageObserver.observe(img);
        });
    }
}

// Back to Top Functions
function initializeBackToTop() {
    var backToTop = $('<button id="backToTop" class="btn btn-primary rounded-circle position-fixed" style="bottom: 20px; right: 20px; display: none; z-index: 1050;"><i class="bi bi-arrow-up"></i></button>');
    $('body').append(backToTop);

    $(window).scroll(function() {
        if ($(this).scrollTop() > 100) {
            backToTop.fadeIn();
        } else {
            backToTop.fadeOut();
        }
    });

    backToTop.click(function() {
        $('html, body').animate({ scrollTop: 0 }, 800);
        return false;
    });
}

// Print Functionality
function initializePrintFunctionality() {
    $(document).on('click', '.print-button', function(e) {
        e.preventDefault();
        window.print();
    });

    // Add print styles dynamically
    var printStyles = `
        <style media="print">
            .navbar, .footer, .btn, .sidebar, #backToTop {
                display: none !important;
            }
            .card {
                box-shadow: none !important;
                border: 1px solid #ddd !important;
            }
            body {
                font-size: 12pt;
                line-height: 1.5;
            }
        </style>
    `;
    $('head').append(printStyles);
}

// Keyboard Shortcuts
function initializeKeyboardShortcuts() {
    $(document).keydown(function(e) {
        // Ctrl + / for search focus
        if (e.ctrlKey && e.key === '/') {
            e.preventDefault();
            $('#bookSearch').focus();
        }
        
        // Ctrl + H for home
        if (e.ctrlKey && e.key === 'h') {
            e.preventDefault();
            window.location.href = '/';
        }
        
        // Ctrl + C for cart
        if (e.ctrlKey && e.key === 'c') {
            e.preventDefault();
            window.location.href = '/Cart';
        }
    });
}

// Utility Functions
function updateCartCount(count) {
    var cartBadge = $('.cart-badge');
    if (cartBadge.length) {
        if (count > 0) {
            cartBadge.text(count).show();
        } else {
            cartBadge.hide();
        }
    }
}

function updateCartTotal(total) {
    $('.cart-total').text('R' + total.toFixed(2));
}

function animateCartIcon() {
    var cartIcon = $('.cart-icon');
    if (cartIcon.length) {
        cartIcon.addClass('bounce');
        setTimeout(function() {
            cartIcon.removeClass('bounce');
        }, 1000);
    }
}

function showToast(type, message) {
    var toastHtml = `
        <div class="toast align-items-center text-white bg-${type} border-0" role="alert" aria-live="assertive" aria-atomic="true">
            <div class="d-flex">
                <div class="toast-body">
                    ${message}
                </div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
            </div>
        </div>
    `;
    
    var toastContainer = $('#toastContainer');
    if (!toastContainer.length) {
        toastContainer = $('<div id="toastContainer" class="toast-container position-fixed bottom-0 end-0 p-3"></div>');
        $('body').append(toastContainer);
    }
    
    toastContainer.append(toastHtml);
    var toast = new bootstrap.Toast(toastContainer.find('.toast').last());
    toast.show();
    
    // Remove toast element after it's hidden
    toastContainer.find('.toast').last().on('hidden.bs.toast', function() {
        $(this).remove();
    });
}

// AJAX Error Handling
$(document).ajaxError(function(event, jqXHR, ajaxSettings, thrownError) {
    console.error('AJAX Error:', thrownError);
    showToast('error', 'An error occurred. Please try again.');
});

// CSRF Token for AJAX requests
$(document).ajaxSend(function(event, jqXHR, ajaxOptions) {
    var token = $('input[name="__RequestVerificationToken"]').val();
    if (token) {
        jqXHR.setRequestHeader('__RequestVerificationToken', token);
    }
});

// Add CSS for animations
var animationStyles = `
    <style>
        @keyframes bounce {
            0%, 20%, 53%, 80%, 100% {
                transform: translate3d(0,0,0);
            }
            40%, 43% {
                transform: translate3d(0, -30px, 0);
            }
            70% {
                transform: translate3d(0, -15px, 0);
            }
            90% {
                transform: translate3d(0, -4px, 0);
            }
        }
        .bounce {
            animation: bounce 1s;
        }
    </style>
`;
$('head').append(animationStyles);