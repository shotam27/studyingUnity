// Vue.js での MonsterSpecies 管理画面の構成例

// 1. メインページコンポーネント
const SpeciesManagementPage = {
  template: `
    <div class="species-management">
      <div class="header">
        <h1>Monster Species Management</h1>
        <div class="actions">
          <button @click="showCreateModal = true" class="btn-primary">
            Add New Species
          </button>
          <button @click="exportData" class="btn-secondary">
            Export Data
          </button>
        </div>
      </div>

      <div class="filters">
        <search-filter 
          v-model="searchQuery" 
          @search="handleSearch"
        />
        <category-filter 
          v-model="selectedFilters" 
          @filter="handleFilter"
        />
      </div>

      <div class="content">
        <species-list 
          :species="speciesList"
          :loading="loading"
          @edit="handleEdit"
          @delete="handleDelete"
          @view="handleView"
        />
        
        <pagination 
          :current-page="currentPage"
          :total-pages="totalPages"
          @page-change="handlePageChange"
        />
      </div>

      <!-- モーダル -->
      <species-create-modal 
        v-if="showCreateModal"
        @close="showCreateModal = false"
        @created="handleSpeciesCreated"
      />
      
      <species-edit-modal 
        v-if="editingSpecies"
        :species="editingSpecies"
        @close="editingSpecies = null"
        @updated="handleSpeciesUpdated"
      />
      
      <confirm-dialog 
        v-if="deleteConfirm"
        :message="deleteConfirm.message"
        @confirm="confirmDelete"
        @cancel="deleteConfirm = null"
      />
    </div>
  `,
  
  data() {
    return {
      speciesList: [],
      searchQuery: '',
      selectedFilters: {},
      currentPage: 1,
      totalPages: 1,
      loading: false,
      showCreateModal: false,
      editingSpecies: null,
      deleteConfirm: null
    }
  },
  
  methods: {
    // データ取得
    async fetchSpecies() {
      this.loading = true
      try {
        const params = {
          page: this.currentPage,
          limit: 20,
          search: this.searchQuery,
          ...this.selectedFilters
        }
        
        const response = await api.getSpeciesList(params)
        this.speciesList = response.data.species
        this.totalPages = response.data.totalPages
      } catch (error) {
        this.$toast.error('Failed to load species data')
      } finally {
        this.loading = false
      }
    },
    
    // 検索処理
    handleSearch(query) {
      this.searchQuery = query
      this.currentPage = 1
      this.fetchSpecies()
    },
    
    // フィルター処理
    handleFilter(filters) {
      this.selectedFilters = filters
      this.currentPage = 1
      this.fetchSpecies()
    },
    
    // 編集処理
    handleEdit(species) {
      this.editingSpecies = { ...species }
    },
    
    // 削除処理
    handleDelete(species) {
      this.deleteConfirm = {
        species,
        message: `Delete species "${species.name}"? This action cannot be undone.`
      }
    },
    
    // 削除確認
    async confirmDelete() {
      try {
        await api.deleteSpecies(this.deleteConfirm.species.id)
        this.$toast.success('Species deleted successfully')
        this.fetchSpecies()
      } catch (error) {
        this.$toast.error('Failed to delete species')
      } finally {
        this.deleteConfirm = null
      }
    },
    
    // データエクスポート
    async exportData() {
      try {
        const blob = await api.exportSpeciesData('json')
        const url = URL.createObjectURL(blob)
        const a = document.createElement('a')
        a.href = url
        a.download = 'monster-species.json'
        a.click()
      } catch (error) {
        this.$toast.error('Failed to export data')
      }
    }
  }
}

// 2. 種族作成モーダル
const SpeciesCreateModal = {
  template: `
    <div class="modal-overlay">
      <div class="modal">
        <div class="modal-header">
          <h2>Create New Species</h2>
          <button @click="$emit('close')" class="close-btn">×</button>
        </div>
        
        <form @submit.prevent="handleSubmit" class="modal-body">
          <div class="form-group">
            <label>Species Name *</label>
            <input 
              v-model="form.name" 
              type="text" 
              required 
              maxlength="50"
              @blur="validateName"
            />
            <div v-if="nameError" class="error">{{ nameError }}</div>
          </div>
          
          <div class="form-group">
            <label>Description</label>
            <textarea v-model="form.description" rows="3"></textarea>
          </div>
          
          <div class="form-row">
            <div class="form-group">
              <label>HP *</label>
              <input v-model.number="form.basicStatus.maxHP" type="number" min="1" max="999" required />
            </div>
            <div class="form-group">
              <label>ATK *</label>
              <input v-model.number="form.basicStatus.atk" type="number" min="0" max="999" required />
            </div>
            <div class="form-group">
              <label>DEF *</label>
              <input v-model.number="form.basicStatus.def" type="number" min="0" max="999" required />
            </div>
            <div class="form-group">
              <label>SPD *</label>
              <input v-model.number="form.basicStatus.spd" type="number" min="0" max="999" required />
            </div>
          </div>
          
          <div class="form-row">
            <div class="form-group">
              <label>Weakness *</label>
              <select v-model="form.weakness" required>
                <option value="">Select weakness</option>
                <option v-for="type in elementTypes" :value="type">{{ type }}</option>
              </select>
            </div>
            <div class="form-group">
              <label>Strength *</label>
              <select v-model="form.strength" required>
                <option value="">Select strength</option>
                <option v-for="type in elementTypes" :value="type">{{ type }}</option>
              </select>
            </div>
          </div>
          
          <div class="form-row">
            <div class="form-group">
              <label>Rarity *</label>
              <select v-model="form.rarity" required>
                <option value="">Select rarity</option>
                <option v-for="rarity in rarityTypes" :value="rarity">{{ rarity }}</option>
              </select>
            </div>
            <div class="form-group">
              <label>Category *</label>
              <select v-model="form.category" required>
                <option value="">Select category</option>
                <option v-for="category in categoryTypes" :value="category">{{ category }}</option>
              </select>
            </div>
          </div>
          
          <div class="form-group">
            <label>Sprite Image</label>
            <image-uploader 
              v-model="form.spriteUrl"
              @upload="handleImageUpload"
            />
          </div>
          
          <div class="form-group">
            <label>Skills</label>
            <skill-selector 
              v-model="form.skillIds"
              :available-skills="availableSkills"
            />
          </div>
        </form>
        
        <div class="modal-footer">
          <button type="button" @click="$emit('close')" class="btn-secondary">
            Cancel
          </button>
          <button 
            type="submit" 
            @click="handleSubmit"
            :disabled="!isValid || saving"
            class="btn-primary"
          >
            {{ saving ? 'Creating...' : 'Create Species' }}
          </button>
        </div>
      </div>
    </div>
  `,
  
  data() {
    return {
      form: {
        name: '',
        description: '',
        basicStatus: {
          maxHP: 100,
          atk: 50,
          def: 50,
          spd: 50
        },
        weakness: '',
        strength: '',
        rarity: '',
        category: '',
        spriteUrl: '',
        skillIds: []
      },
      nameError: '',
      saving: false,
      elementTypes: ['Fire', 'Water', 'Ice', 'Electric', 'Earth', 'Air', 'Light', 'Dark'],
      rarityTypes: ['Common', 'Uncommon', 'Rare', 'Epic', 'Legendary'],
      categoryTypes: ['Beast', 'Dragon', 'Elemental', 'Humanoid', 'Undead', 'Plant', 'Machine', 'Spirit'],
      availableSkills: []
    }
  },
  
  computed: {
    isValid() {
      return this.form.name && 
             this.form.basicStatus.maxHP > 0 &&
             this.form.weakness &&
             this.form.strength &&
             this.form.rarity &&
             this.form.category &&
             !this.nameError
    }
  },
  
  methods: {
    async validateName() {
      if (!this.form.name) return
      
      try {
        const response = await api.validateSpeciesName(this.form.name)
        this.nameError = response.data.exists ? 'Species name already exists' : ''
      } catch (error) {
        console.error('Name validation failed:', error)
      }
    },
    
    async handleImageUpload(file) {
      try {
        const response = await api.uploadSprite(file)
        this.form.spriteUrl = response.data.url
      } catch (error) {
        this.$toast.error('Failed to upload image')
      }
    },
    
    async handleSubmit() {
      if (!this.isValid) return
      
      this.saving = true
      try {
        const response = await api.createSpecies(this.form)
        this.$toast.success('Species created successfully')
        this.$emit('created', response.data)
        this.$emit('close')
      } catch (error) {
        this.$toast.error('Failed to create species')
      } finally {
        this.saving = false
      }
    }
  }
}

// 3. API サービス
const api = {
  async getSpeciesList(params) {
    const queryString = new URLSearchParams(params).toString()
    return await fetch(`${API_BASE}/species?${queryString}`)
      .then(response => response.json())
  },
  
  async createSpecies(data) {
    return await fetch(`${API_BASE}/species`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(data)
    }).then(response => response.json())
  },
  
  async updateSpecies(id, data) {
    return await fetch(`${API_BASE}/species/${id}`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(data)
    }).then(response => response.json())
  },
  
  async deleteSpecies(id) {
    return await fetch(`${API_BASE}/species/${id}`, {
      method: 'DELETE'
    }).then(response => response.json())
  },
  
  async validateSpeciesName(name) {
    return await fetch(`${API_BASE}/species/validate-name`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ name })
    }).then(response => response.json())
  },
  
  async uploadSprite(file) {
    const formData = new FormData()
    formData.append('sprite', file)
    
    return await fetch(`${API_BASE}/species/sprites`, {
      method: 'POST',
      body: formData
    }).then(response => response.json())
  },
  
  async exportSpeciesData(format) {
    return await fetch(`${API_BASE}/species/export?format=${format}`)
      .then(response => response.blob())
  }
}
