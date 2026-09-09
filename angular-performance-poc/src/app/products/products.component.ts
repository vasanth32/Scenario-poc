import { Component, OnInit, inject } from '@angular/core';
import { ProductService, Product } from '../services/product.service';

@Component({
  selector: 'app-products',
  standalone: true,
  templateUrl: './products.component.html',
  styleUrl: './products.component.css'
})

export class ProductsComponent implements OnInit {

   private productService = inject(ProductService);
   products: Product[] = [];
   loading = false;
    
    ngOnInit(): void {
    this.loadProducts();
    }
    loadProducts(): void {

    this.loading = true;

    this.productService.getProducts()
      .subscribe({
        next: response => {

          this.products = response.products;

          this.loading = false;

          console.log('Products:', response);

        },
        error: error => {

          console.error('API Error:', error);

          this.loading = false;

        }
      });

    }
}
