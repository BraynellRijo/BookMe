import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';

export interface PropertyCategory {
  id: string;
  label: string;
  icon: string;
}

@Component({
  selector: 'app-category-filter',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './category-filter.html',
  styleUrls: ['./category-filter.css']
})
export class CategoryFilterComponent {
  @Input() activeCategoryId: string = 'All';
  @Output() categorySelected = new EventEmitter<string>();

  categories: PropertyCategory[] = [
    { id: 'All', label: 'Todo', icon: 'apps' },
    { id: 'House', label: 'Casas', icon: 'house' },
    { id: 'Apartment', label: 'Apartamentos', icon: 'apartment' },
    { id: 'Villa', label: 'Villas', icon: 'holiday_village' },
    { id: 'Cabin', label: 'Cabañas', icon: 'cottage' },
    { id: 'Cottage', label: 'Cottages', icon: 'night_shelter' },
    { id: 'Chalet', label: 'Chalets', icon: 'chalet' },
    { id: 'Townhouse', label: 'Townhouses', icon: 'other_houses' },
    { id: 'Resort', label: 'Resorts', icon: 'pool' },
    { id: 'Treehouse', label: 'Treehouses', icon: 'forest' },
    { id: 'Castle', label: 'Castillos', icon: 'castle' },
  ];

  selectCategory(id: string) {
    this.categorySelected.emit(id);
  }
}
